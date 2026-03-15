using System.Collections.Generic;
using Isusov.Time.Scheduling;
using NUnit.Framework;

namespace Isusov.Time.Tests.Runtime
{
  public sealed class SimulationSchedulerTests
  {
    [Test]
    public void ExecuteDue_SameTickCallbacks_RunInRegistrationOrder()
    {
      var scheduler = new SimulationScheduler();
      var executionOrder = new List<int>();

      scheduler.ScheduleAt(new GameTick(5L), () => executionOrder.Add(1));
      scheduler.ScheduleAt(new GameTick(5L), () => executionOrder.Add(2));
      scheduler.ScheduleAt(new GameTick(5L), () => executionOrder.Add(3));

      var executedCount = scheduler.ExecuteDue(new GameTick(5L));

      Assert.That(executedCount, Is.EqualTo(3));
      Assert.That(executionOrder, Is.EqualTo(new[] { 1, 2, 3 }));
      Assert.That(scheduler.PendingCount, Is.EqualTo(0));
    }

    [Test]
    public void Cancel_RemovesOnlyMatchingHandleFromSharedTickBucket()
    {
      var scheduler = new SimulationScheduler();
      var executionOrder = new List<int>();

      var cancelledHandle = scheduler.ScheduleAt(new GameTick(5L), () => executionOrder.Add(1));
      var remainingHandle = scheduler.ScheduleAt(new GameTick(5L), () => executionOrder.Add(2));

      var cancelled = scheduler.Cancel(cancelledHandle);
      var executedCount = scheduler.ExecuteDue(new GameTick(5L));

      Assert.That(cancelled, Is.True);
      Assert.That(scheduler.Contains(cancelledHandle), Is.False);
      Assert.That(scheduler.Contains(remainingHandle), Is.False);
      Assert.That(executedCount, Is.EqualTo(1));
      Assert.That(executionOrder, Is.EqualTo(new[] { 2 }));
    }
  }
}