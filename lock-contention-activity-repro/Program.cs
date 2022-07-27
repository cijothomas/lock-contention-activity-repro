// <copyright file="Program.cs" company="OpenTelemetry Authors">
// Copyright The OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using OpenTelemetry;
using OpenTelemetry.Exporter.Geneva;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Net;

public class Program
{
    private static ActivitySource mysource = new ActivitySource("mysource");

    public static void Main()
    {
        long activityCount = 0;
        using var meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddRuntimeInstrumentation()
            .AddPrometheusExporter(options =>
            {
                options.StartHttpListener = true;
                options.ScrapeResponseCacheDurationMilliseconds = 0;
            })
            .Build();

        var items = new List<Activity>();

        ActivityListener listener = new ActivityListener();
        listener.ShouldListenTo = (source) => source.Name.Equals("mysource");
        listener.Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllDataAndRecorded;
        listener.ActivityStopped = activity => Interlocked.Increment(ref activityCount);
        ActivitySource.AddActivityListener(listener);

        Console.WriteLine(".NET Runtime metrics are available at http://localhost:9464/metrics");

        Parallel.Invoke(() => { StartAndStopActivity(); }, () => { StartAndStopActivity();  });               
    }

    private static void StartAndStopActivity()
    {
        while (true)
        {
            using (var activity = mysource.StartActivity("IsThereContention"))
            {
                activity.DisplayName = "changedName";
                activity.SetTag("Description", "Fix cert issues in ABC region");
                activity.SetTag("Priority", 1);
            }
        }
    }
}
