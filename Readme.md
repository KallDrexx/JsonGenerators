# Json Generators

Proof of concept code to use source generators to generate JSON deserialization code for a class at compile time.

```
    [GenerateJsonDeserializer]
    public class TestClass
    {
        public int NumericValue { get; set; }
        public string SomeOtherContent { get; set; }
        public double DecimalValue { get; set; }
        public int? MaybeValue { get; set; }
        public bool IsCorrect { get; set; }
    }

    //...
    var value = TestClassDeserializer.Deserialize(json);
```

Benchmarks:

```
|             Method |     Mean |     Error |    StdDev |   Median |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------- |---------:|----------:|----------:|---------:|-------:|------:|------:|----------:|
| CustomDeserializer | 1.237 us | 0.1539 us | 0.4539 us | 1.054 us | 0.0381 |     - |     - |     248 B |
|         JsonDotNet | 3.320 us | 0.1884 us | 0.5495 us | 3.342 us | 0.4482 |     - |     - |    2824 B |
|     SystemTextJson | 1.176 us | 0.0850 us | 0.2506 us | 1.142 us | 0.0134 |     - |     - |      88 B |
```