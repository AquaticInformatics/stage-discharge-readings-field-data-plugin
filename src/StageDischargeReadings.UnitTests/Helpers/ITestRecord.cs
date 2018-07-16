using StageDischargeReadings.Interfaces;

namespace StageDischargeReadings.UnitTests.Helpers
{
    public interface ITestRecord<out TRecord> where TRecord : ISelfValidator
    {
        TRecord AParametricRecord(int ordinal);
    }
}
