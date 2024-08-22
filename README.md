# Building pipelines with IAsyncEnumerable

[![Build](https://github.com/NikiforovAll/async-enumerable-pipelines/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/NikiforovAll/async-enumerable-pipelines/actions/workflows/build.yml)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/nikiforovall/async-enumerable-pipelines/blob/main/LICENSE.md)

This repository demonstrates how to use `IAsyncEnumerable` and `System.Linq.Async` to build pipelines in C#.


> [!IMPORTANT]
> This repository doesn't cover all the possible concerns such as error handling, cancellation, backpressure, performance, etc. It's just a simple demonstration of how to build pipelines with `IAsyncEnumerable` and `System.Linq.Async`. 

```bash
dotnet example --list
```

```text
╭─────────────────────────────────────────┬────────────────────────────────────────────────────────────────────────────────────────────╮
│ Example                                 │ Description                                                                                │
├─────────────────────────────────────────┼────────────────────────────────────────────────────────────────────────────────────────────┤
│ CalculateWordCountPipeline              │ Demonstrates how to build async-enumerable pipelines based on standard LINQ operators      │
│ CalculateWordCountFileWatcherPipeline   │ Demonstrates how to combine async-enumerable pipelines with IObservable. E.g: file watcher │
│ CalculateWordCountBatchPipeline         │ Demonstrates how to use batching in async-enumerable pipelines                             │
│ TextSummarizationAndAggregationPipeline │ Demonstrates how to build custom async-enumerable operators                                │
╰─────────────────────────────────────────┴────────────────────────────────────────────────────────────────────────────────────────────╯
```
## Demo: CalculateWordCountPipeline

<video src="https://github.com/user-attachments/assets/84c1e8a8-996d-4960-9b39-20e6bd1101a9" controls="controls"></video>

## Demo: CalculateWordCountFileWatcherPipeline

<video src="https://github.com/user-attachments/assets/56db32bd-a7e9-41ec-8706-eaf876750bb6" controls="controls"></video>

## Demo: CalculateWordCountBatchPipeline

<video src="https://github.com/user-attachments/assets/96cc653d-8b42-4779-b2f2-fce804f0160b" controls="controls"></video>

## Demo: TextSummarizationAndAggregationPipeline

<video src="https://github.com/user-attachments/assets/42c6eb97-7a11-4b89-857e-1ffb8e70073c" controls="controls"></video>
