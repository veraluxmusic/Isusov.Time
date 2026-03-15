using Isusov.Time.Calendar;
using Isusov.Time.Config;
using NUnit.Framework;

namespace Isusov.Time.Tests.Runtime
{
  public sealed class WorldTimeConfigTests : WorldTimeTestBase
  {

    [Test]
    public void CalendarDefinition_WhenAuthoredFieldIsNull_ResolvesDefaultWithoutMutatingSerializedState()
    {
      var config = CreateConfig();
      SetPrivateField(config, "calendarDefinition", null);

      var resolvedCalendar = config.CalendarDefinition;
      var isValid = config.TryValidate(out var errorMessage);

      Assert.That(resolvedCalendar, Is.Not.Null);
      Assert.That(isValid, Is.True, errorMessage);
      Assert.That(GetPrivateField<CalendarDefinition>(config, "calendarDefinition"), Is.Null);
      Assert.That(config.UsesImplicitDefaultCalendar, Is.True);
    }

    [Test]
    public void CreateTimeMapping_WithImplicitDefaultCalendar_UsesGregorianCalendarWithoutMutatingSerializedState()
    {
      var config = CreateConfig();
      SetPrivateField(config, "calendarDefinition", null);

      var timeMapping = config.CreateTimeMapping();

      Assert.That(timeMapping.CalendarDefinition, Is.Not.Null);
      Assert.That(timeMapping.GetDate(timeMapping.GetStartTickForDate(config.StartDate)), Is.EqualTo(config.StartDate));
      Assert.That(GetPrivateField<CalendarDefinition>(config, "calendarDefinition"), Is.Null);
    }
  }
}