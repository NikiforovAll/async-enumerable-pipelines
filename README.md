# Building pipelines with IAsyncEnumerable

[![Build](https://github.com/NikiforovAll/async-enumerable-pipelines/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/NikiforovAll/async-enumerable-pipelines/actions/workflows/build.yml)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/nikiforovall/async-enumerable-pipelines/blob/main/LICENSE.md)

This repository demonstrates how to use `IAsyncEnumerable` and `System.Linq.Async` to build pipelines in C#.

```bash
dotnet example --list
```

```text
╭─────────────────────────────────────────┬────────────────────────────────────────────────────────────────────────────────────────────╮
│ Example                                 │ Description                                                                                │
├─────────────────────────────────────────┼────────────────────────────────────────────────────────────────────────────────────────────┤
│ CalculateWordCountPipeline              │ Demonstrates how to build async-enumerable pipelines based on standard LINQ operators      │
│ CalculateWordCountBatchPipeline         │ Demonstrates how to use batching in async-enumerable pipelines                             │
│ CalculateWordCountFileWatcherPipeline   │ Demonstrates how to combine async-enumerable pipelines with IObservable. E.g: file watcher │
│ TextSummarizationAndAggregationPipeline │ Demonstrates how to build custom async-enumerable operators.                               │
╰─────────────────────────────────────────┴────────────────────────────────────────────────────────────────────────────────────────────╯
```
