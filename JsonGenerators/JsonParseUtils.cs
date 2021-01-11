using System;

namespace JsonGenerators
{
    public static class JsonParseUtils
    {
        public delegate int? PropertySetMethod<T>(T instance, ReadOnlySpan<char> json, JsonSection section) where T : class;
        
        public static JsonSection FindNextString(ReadOnlySpan<char> json, int startIndex)
        {
            var firstCharAt = (int?) null;
            var lastCharAt = (int?) null;
            var inEscapeMode = false;
            var currentIndex = startIndex;
            while(currentIndex < json.Length && lastCharAt == null)
            {
                switch (json[currentIndex])
                {
                    case '"' when firstCharAt == null:
                        firstCharAt = currentIndex;
                        break;
                    
                    case '"' when !inEscapeMode:
                        lastCharAt = currentIndex;
                        break;
                    
                    case var ch when char.IsWhiteSpace(ch):
                        break;
                    
                    case 'n' when firstCharAt != null &&
                                  currentIndex + 3 < json.Length &&
                                  json[currentIndex + 1] == 'u' &&
                                  json[currentIndex + 2] == 'l' &&
                                  json[currentIndex + 3] == 'l':
                        firstCharAt = currentIndex;
                        lastCharAt = currentIndex + 4;
                        currentIndex = lastCharAt.Value;
                        break;
                    
                    case var _ when firstCharAt == null:
                        throw new InvalidOperationException($"Unexpected token at index {currentIndex}");
                    
                    case '\\' when !inEscapeMode:
                        inEscapeMode = true;
                        break;
                    
                    case var _ when inEscapeMode:
                        inEscapeMode = false;
                        break;
                }

                currentIndex++;
            }

            if (firstCharAt == null)
            {
                throw new InvalidOperationException("No string value found");
            }

            if (lastCharAt == null)
            {
                throw new InvalidOperationException("Open quote without ending quote");
            }

            var nextCharAt = GetNextCharIndex(json, currentIndex);
            return new JsonSection(firstCharAt.Value, lastCharAt.Value, nextCharAt);
        }

        public static JsonSection FindNextNumber(ReadOnlySpan<char> json, int startIndex)
        {
            var firstCharAt = (int?) null;
            var lastCharAt = (int?) null;
            var decimalEncountered = false;
            var currentIndex = startIndex;
            while (currentIndex < json.Length && lastCharAt == null)
            {
                switch (json[currentIndex])
                {
                    case '.' when firstCharAt == null || decimalEncountered:
                        throw new InvalidOperationException($"Unexpected character at index {currentIndex}");
                    
                    case '.' when !decimalEncountered:
                        decimalEncountered = true;
                        break;
                    
                    case 'n' when firstCharAt == null &&
                                json.Length > currentIndex + 3 &&
                                json[currentIndex + 1] == 'u' &&
                                json[currentIndex + 2] == 'l' &&
                                json[currentIndex + 3] == 'l':
                        firstCharAt = currentIndex;
                        lastCharAt = currentIndex + 4;
                        currentIndex = lastCharAt.Value;
                        break;
                    
                    case var ch when char.IsDigit(ch):
                        firstCharAt ??= currentIndex;
                        break;
                    
                    case var ch when char.IsWhiteSpace(ch) && firstCharAt == null:
                        break;
                    
                    case var ch when firstCharAt != null && !char.IsDigit(ch):
                        lastCharAt = currentIndex;
                        break;
                    
                    default:
                        throw new InvalidOperationException($"Unexpected character at index {currentIndex}");
                }

                currentIndex++;
            }

            if (firstCharAt == null || lastCharAt == null)
            {
                throw new InvalidOperationException("No numeric value found");
            }
            
            var nextCharAt = GetNextCharIndex(json, lastCharAt.Value);
            return new JsonSection(firstCharAt.Value, lastCharAt.Value, nextCharAt);
        }

        public static JsonSection FindNextBool(ReadOnlySpan<char> json, int startIndex)
        {
            var firstCharAt = (int?) null;
            var lastCharAt = (int?) null;
            var currentIndex = startIndex;
            while (currentIndex < json.Length && lastCharAt == null)
            {
                switch (json[currentIndex])
                {
                    case var ch when char.IsWhiteSpace(ch):
                        break;
                    
                    case 'n' when json.Length > currentIndex + 3 &&
                                  json[currentIndex + 1] == 'u' &&
                                  json[currentIndex + 2] == 'l' &&
                                  json[currentIndex + 3] == 'l':
                        firstCharAt = currentIndex;
                        lastCharAt = currentIndex + 4;
                        currentIndex = lastCharAt.Value;
                        break;
                    
                    case 't' when json.Length > currentIndex + 3 &&
                                  json[currentIndex + 1] == 'r' &&
                                  json[currentIndex + 2] == 'u' &&
                                  json[currentIndex + 3] == 'e':
                        firstCharAt = currentIndex;
                        lastCharAt = currentIndex + 4;
                        currentIndex = lastCharAt.Value;
                        break;
                    
                    case 'f' when json.Length > currentIndex + 4 &&
                                  json[currentIndex + 1] == 'a' &&
                                  json[currentIndex + 2] == 'l' &&
                                  json[currentIndex + 3] == 's' &&
                                  json[currentIndex + 4] == 'e':
                        firstCharAt = currentIndex;
                        lastCharAt = currentIndex + 5;
                        currentIndex = lastCharAt.Value;
                        break;
                    
                    default:
                        throw new InvalidOperationException($"Unexpected character rat index {currentIndex}");
                }
            }

            if (firstCharAt == null || lastCharAt == null)
            {
                throw new InvalidOperationException("No boolean value found");
            }
            
            var nextCharAt = GetNextCharIndex(json, currentIndex);
            return new JsonSection(firstCharAt.Value, lastCharAt.Value, nextCharAt);
        }

        public static int? GetNextCharIndex(ReadOnlySpan<char> json, int startIndex)
        {
            for (var x = startIndex; x < json.Length; x++)
            {
                if (!char.IsWhiteSpace(json[x]))
                {
                    return x;
                }
            }

            return null;
        }

        public static T ParseObject<T>(ReadOnlySpan<char> json, int startIndex, PropertySetMethod<T> propertySetMethod) 
            where T : class, new()
        {
            var result = (T) null;

            var firstCharAt = JsonParseUtils.GetNextCharIndex(json, startIndex);
            if (firstCharAt == null)
            {
                // All whitespace
                return null;
            }

            switch (json[firstCharAt.Value])
            {
                case 'n' when json.Length >= 4 &&
                            json[1] == 'u' &&
                            json[2] == 'l' &&
                            json[3] == 'l':
                    var followingIndex = JsonParseUtils.GetNextCharIndex(json, 4);
                    if (followingIndex != null)
                    {
                        throw new InvalidOperationException($"Unexpected character '{json[followingIndex.Value]}' at index {followingIndex.Value}");
                    }

                    // explicit null
                    return null;
                
                case '{':
                    result = new T();
                    break;
                
                default:
                    throw new InvalidOperationException($"Unexpected character '{json[firstCharAt.Value]}' at index {firstCharAt.Value}");
            }

            // We are in an object declaration, as expected.  Start parsing the properties one by one
            var nextCharAt = JsonParseUtils.GetNextCharIndex(json, 1);
            while (nextCharAt != null)
            {
                switch (json[nextCharAt.Value])
                {
                    case '}':
                        nextCharAt = GetNextCharIndex(json, nextCharAt.Value + 1);
                        if (nextCharAt != null)
                        {
                            throw new InvalidOperationException($"Unexpected character '{json[nextCharAt.Value]}' at index {nextCharAt.Value}");
                        }
                    
                        return result;
                    
                    case '"':
                        var section = FindNextString(json, nextCharAt.Value);

                        // Should be followed by a colon
                        if (section.NextCharIndex == null)
                        {
                            var message = $"Unexpected end of string found after property starting at {startIndex}";
                            throw new InvalidOperationException(message);
                        }

                        if (json[section.NextCharIndex.Value] != ':')
                        {
                            var message = $"Expected colon but found '{json[section.NextCharIndex.Value]}' at index {section.NextCharIndex}";
                            throw new InvalidOperationException(message);
                        }
                        
                        nextCharAt = propertySetMethod(result, json, section);
                        break;
                    
                    case ',':
                        nextCharAt = GetNextCharIndex(json, nextCharAt.Value + 1);
                        break;
                    
                    default:
                        throw new InvalidOperationException($"Unexpected character '{json[nextCharAt.Value]}' at index {nextCharAt.Value}");
                }
            }

            throw new InvalidOperationException("Unexpected end of string encountered");
        }
    }
}