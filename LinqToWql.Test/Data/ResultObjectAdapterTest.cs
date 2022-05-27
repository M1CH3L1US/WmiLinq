using System.Reflection;
using LinqToWql.Data;
using LinqToWql.Infrastructure;
using LinqToWql.Test.Mocks.Resources;
using LinqToWql.Test.Mocks.ResultObject;
using Microsoft.ConfigurationManagement.ManagementProvider;
using Moq;
using Xunit.Sdk;

namespace LinqToWql.Test.Data;

public class ResultObjectAdapterTest {
  [Fact]
  public void GetProperty_GetsValueFromStringValue_WhenTypeIsString() {
    AssertInvocationForSimpleProperty(
      sut => sut.GetProperty<string>("StringProperty"),
      invocation => invocation.Method.IsGetterWithName(nameof(IQueryPropertyItem.StringValue))
    );
  }

  [Fact]
  public void SetProperty_SetsValueInStringValue_WhenTypeIsString() {
    AssertInvocationForSimpleProperty(
      sut => sut.SetProperty<string>("StringProperty", "StringValue"),
      invocation => invocation.Method.IsSetterWithName(nameof(IQueryPropertyItem.StringValue))
    );
  }

  [Fact]
  public void GetProperty_GetsValueFromObjectValue_WhenTypeIsUnknownTypeAndNotResource() {
    AssertInvocationForSimpleProperty(
      sut => sut.GetProperty<IInvocation>("SomeProperty"),
      invocation => invocation.Method.IsGetterWithName(nameof(IQueryPropertyItem.ObjectValue))
    );
  }

  [Fact]
  public void SetProperty_SetsValueInObjectValue_WhenTypeIsUnknownTypeAndNotResource() {
    AssertInvocationForSimpleProperty(
      sut => sut.SetProperty<IInvocation>("SomeProperty", null!),
      invocation => invocation.Method.IsSetterWithName(nameof(IQueryPropertyItem.ObjectValue))
    );
  }

  [Fact]
  public void GetProperty_GetsValueFromStringArrayValue_WhenTypeIsIEnumerableString() {
    AssertInvocationForSimpleProperty(
      sut => sut.GetProperty<IEnumerable<string>>("SomeProperty"),
      invocation => invocation.Method.IsGetterWithName(nameof(IQueryPropertyItem.StringArrayValue))
    );
  }

  [Fact]
  public void SetProperty_SetsValueFromStringArrayValue_WhenTypeIsIEnumerableString() {
    AssertInvocationForSimpleProperty(
      sut => sut.SetProperty("SomeProperty", Enumerable.Empty<string>()),
      invocation => invocation.Method.IsSetterWithName(nameof(IQueryPropertyItem.StringArrayValue))
    );
  }

  [Fact]
  public void GetProperty_GetsValueFromObjectArrayValue_WhenTypeIsUnknownNonResourceEnumerable() {
    AssertInvocationForSimpleProperty(
      sut => sut.GetProperty<IEnumerable<IInvocation>>("SomeProperty"),
      invocation => invocation.Method.IsGetterWithName(nameof(IQueryPropertyItem.ObjectArrayValue))
    );
  }

  [Fact]
  public void SetProperty_SetsValueFromObjectArrayValue_WhenTypeIsUnknownNonResourceEnumerable() {
    AssertInvocationForSimpleProperty(
      sut => sut.SetProperty("SomeProperty", Enumerable.Empty<IInvocation>()),
      invocation => invocation.Method.IsSetterWithName(nameof(IQueryPropertyItem.ObjectArrayValue))
    );
  }

  [Fact]
  public void SetProperty_ConvertsValueEnumerableToArray_WhenSettingValueForIEnumerableProperty() {
    AssertInvocationForSimpleProperty(
      sut => sut.SetProperty("SomeProperty", Enumerable.Empty<IInvocation>()),
      invocation => invocation.Arguments.First().Should().BeOfType<IInvocation[]>()
    );
  }

  private void AssertInvocationForSimpleProperty(
    Action<ResultObjectAdapter> accessAction,
    Action<IInvocation> validationAction
  ) {
    var resultObjectMock = new Mock<IResultObject>();
    var propertyMock = new Mock<IQueryPropertyItem>();
    resultObjectMock.Setup(o => o[It.IsAny<string>()]).Returns(propertyMock.Object);

    var sut = new ResultObjectAdapter(null, resultObjectMock.Object);

    accessAction(sut);
    validationAction(propertyMock.Invocations.First());
  }

  [Fact]
  public void GetProperty_CallsGetSingleItemOfResultObject_WhenTypeIsResource() {
    AssertInvocationForEmbeddedProperty(
      sut => sut.GetProperty<SmsCollection>("Collection"),
      objectInvocation => objectInvocation.Method.Name.Should().Be(nameof(IResultObject.GetSingleItem))
    );
  }

  [Fact]
  public void GetProperty_WrapsInnerResultObjectToResource_WhenTypeIsResource() {
    AssertInvocationForEmbeddedProperty(
      sut => sut.GetProperty<SmsCollection>("Collection").Should().BeAssignableTo<SmsCollection>()
    );
  }

  [Fact]
  public void SetProperty_CallsSetSingleItemOfResultObject_WhenTypeIsResource() {
    AssertInvocationForEmbeddedProperty(
      sut => sut.SetProperty("Collection", MakeNoopResource()),
      objectInvocation => objectInvocation.Method.Name.Should().Be(nameof(IResultObject.SetSingleItem))
    );
  }

  [Fact]
  public void SetProperty_CallsSetSingleItemOfResultObjectWithWrappedResultObject_WhenTypeIsResource() {
    AssertInvocationForEmbeddedProperty(
      sut => sut.SetProperty("Collection", MakeNoopResource()),
      objectInvocation => objectInvocation.Arguments.Skip(1).First().Should().BeAssignableTo<IResultObject>()
    );
  }

  [Fact]
  public void GetProperty_CallsGetArrayItemsOfResultObject_WhenTypeIsEnumerableResource() {
    AssertInvocationForEmbeddedProperty(
      sut => sut.GetProperty<IEnumerable<SmsCollection>>("Collection"),
      objectInvocation => objectInvocation.Method.Name.Should().Be(nameof(IResultObject.GetArrayItems))
    );
  }

  [Fact]
  public void GetProperty_WrapsAllResultObjectsToResource_WhenTypeIsEnumerableResource() {
    AssertInvocationForEmbeddedProperty(
      sut => sut.GetProperty<IEnumerable<SmsCollection>>("Collection").All(x => x is SmsCollection).Should().BeTrue()
    );
  }

  [Fact]
  public void GetProperty_AcceptsDerivedTypesOfIEnumerable_WhenTypeIsEnumerableResource() {
    AssertInvocationForEmbeddedProperty(
      sut => sut.GetProperty<List<SmsCollection>>("Collection"),
      objectInvocation => objectInvocation.Method.Name.Should().Be(nameof(IResultObject.GetArrayItems))
    );
  }

  [Fact]
  public void GetProperty_AlwaysReturnsListInstance_WhenTypeIsEnumerableResource() {
    AssertInvocationForEmbeddedProperty(
      sut => sut.GetProperty<List<SmsCollection>>("Collection"),
      objectInvocation => objectInvocation.Method.Name.Should().Be(nameof(IResultObject.GetArrayItems))
    );
  }

  [Fact]
  public void SetProperty_CallsSetArrayItemsOfResultObject_WhenTypeIsEnumerableResource() {
    AssertInvocationForEmbeddedProperty(
      sut => sut.SetProperty<IEnumerable<SmsCollection>>("Collection", new[] {MakeNoopResource()}),
      objectInvocation => objectInvocation.Method.Name.Should().Be(nameof(IResultObject.SetArrayItems))
    );
  }

  [Fact]
  public void SetProperty_CastsSetValueToListOfResource_WhenTypeIsEnumerableResource() {
    AssertInvocationForEmbeddedProperty(
      sut => sut.SetProperty("Collection", new List<SmsCollection> {MakeNoopResource()}),
      objectInvocation => objectInvocation.Arguments.Skip(1).First().Should().BeOfType<List<IResultObject>>()
    );
  }

  private SmsCollection MakeNoopResource() {
    return new SmsCollection(new ResourceObject(new ResourceObjectOptions()));
  }

  private void AssertInvocationForEmbeddedProperty(
    Action<ResultObjectAdapter> accessAction,
    Action<IInvocation>? resultObjectMethodInvocationAction = null,
    Action<IInvocation>? contextMethodInvocationAction = null
  ) {
    var resultObjectMock = new Mock<IResultObject>();
    var itemMock = new Mock<IResultObject>();

    resultObjectMock.Setup(o => o.GetSingleItem(It.IsAny<string>())).Returns(itemMock.Object);
    resultObjectMock.Setup(o => o.GetArrayItems(It.IsAny<string>())).Returns(new List<IResultObject> {itemMock.Object});

    var contextMock = new Mock<WqlResourceContext>(null);
    var sut = new ResultObjectAdapter(contextMock.Object, resultObjectMock.Object);

    accessAction(sut);
    resultObjectMethodInvocationAction?.Invoke(resultObjectMock.Invocations.First());
    contextMethodInvocationAction?.Invoke(contextMock.Invocations.First());
  }
}

internal static class InvocationListExtensions {
  public static void IsGetterWithName(this MethodInfo methodInfo, string propertyName) {
    if (!methodInfo.Name.StartsWith("get_")) {
      throw new XunitException($"Expected {methodInfo.Name} to be getter");
    }

    var getterName = methodInfo.Name.Replace("get_", "");
    getterName.Should().Be(propertyName);
  }

  public static void IsSetterWithName(this MethodInfo methodInfo, string propertyName) {
    if (!methodInfo.Name.StartsWith("set_")) {
      throw new XunitException($"Expected {methodInfo.Name} to be setter");
    }

    var setterName = methodInfo.Name.Replace("set_", "");
    setterName.Should().Be(propertyName);
  }
}