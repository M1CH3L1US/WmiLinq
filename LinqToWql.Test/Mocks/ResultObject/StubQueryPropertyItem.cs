using System.Windows.Forms;
using Microsoft.ConfigurationManagement.AdminConsole.Schema;
using Microsoft.ConfigurationManagement.ManagementProvider;

namespace LinqToWql.Test.Mocks.ResultObject;

public class StubQueryPropertyItem : IQueryPropertyItem {
  private object _value { get; set; }

  public ManagementClassPropertyDescription.TypeOfData DataType { get; }
  public bool IsArray { get; }
  public string[] QualifierNames { get; }
  public byte[] ByteArrayValue { get; set; }
  public CheckState CheckStateValue { get; set; }
  public DateTime DateTimeValue { get; set; }
  public DateTime[] DateTimeArrayValue { get; set; }
  public DateTime TimeValue { get; set; }
  public TimeSpan TimeSpanValue { get; set; }

  public bool BooleanValue {
    get => (bool) _value;
    set => _value = value;
  }

  public bool[] BooleanArrayValue {
    get => (bool[]) _value;
    set => _value = value;
  }

  public int IntegerValue {
    get => (int) _value;
    set => _value = value;
  }

  public int[] IntegerArrayValue {
    get => (int[]) _value;
    set => _value = value;
  }

  public long LongValue {
    get => (long) _value;
    set => _value = value;
  }

  public string StringValue {
    get => (string) _value;
    set => _value = value;
  }

  public string[] StringArrayValue {
    get => (string[]) _value;
    set => _value = value;
  }

  public object ObjectValue {
    get => _value;
    set => _value = value;
  }

  public object[] ObjectArrayValue {
    get => (object[]) _value;
    set => _value = value;
  }

  public string CombinedStringValue { get; }

  public StubQueryPropertyItem(object value) {
    _value = value;
  }

  public object Clone() {
    throw new NotImplementedException();
  }

  public BitFlag Bit(int mask, int checkedState) {
    throw new NotImplementedException();
  }

  public int GetBitValue(int mask) {
    throw new NotImplementedException();
  }

  public void SetBitValue(int mask, bool setFlag) {
    throw new NotImplementedException();
  }
}