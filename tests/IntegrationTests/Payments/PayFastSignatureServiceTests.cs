using Infrastructure.Payments;

namespace IntegrationTests.Payments;

public sealed class PayFastSignatureServiceTests
{
    [Fact]
    public void Sign_Should_ProduceSameSignature_ForSameFieldsAndPassphrase()
    {
        // Arrange
        var service = new PayFastSignatureService();
        var fields = new List<KeyValuePair<string, string>>
        {
            new("merchant_id", "10000100"),
            new("merchant_key", "46f0cd694581a"),
            new("amount", "25.00"),
            new("item_name", "ekasi-property monthly listing subscription")
        };

        // Act
        string signatureA = service.Sign(fields, "test-passphrase");
        string signatureB = service.Sign(fields, "test-passphrase");

        // Assert
        signatureA.ShouldBe(signatureB);
        signatureA.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Sign_Should_ProduceDifferentSignature_WhenAFieldValueChanges()
    {
        // Arrange
        var service = new PayFastSignatureService();
        var fields = new List<KeyValuePair<string, string>>
        {
            new("merchant_id", "10000100"),
            new("amount", "25.00")
        };
        var changedFields = new List<KeyValuePair<string, string>>
        {
            new("merchant_id", "10000100"),
            new("amount", "50.00")
        };

        // Act
        string original = service.Sign(fields, "test-passphrase");
        string changed = service.Sign(changedFields, "test-passphrase");

        // Assert
        original.ShouldNotBe(changed);
    }

    [Fact]
    public void Sign_Should_IgnoreEmptyFieldValues()
    {
        // Arrange
        var service = new PayFastSignatureService();
        var withEmpty = new List<KeyValuePair<string, string>>
        {
            new("merchant_id", "10000100"),
            new("optional_field", string.Empty),
            new("amount", "25.00")
        };
        var withoutEmpty = new List<KeyValuePair<string, string>>
        {
            new("merchant_id", "10000100"),
            new("amount", "25.00")
        };

        // Act
        string signatureWithEmpty = service.Sign(withEmpty, "test-passphrase");
        string signatureWithoutEmpty = service.Sign(withoutEmpty, "test-passphrase");

        // Assert
        signatureWithEmpty.ShouldBe(signatureWithoutEmpty);
    }

    [Fact]
    public void Sign_Should_ProduceDifferentSignature_WhenPassphraseChanges()
    {
        // Arrange
        var service = new PayFastSignatureService();
        var fields = new List<KeyValuePair<string, string>> { new("merchant_id", "10000100") };

        // Act
        string withPassphrase = service.Sign(fields, "test-passphrase");
        string withoutPassphrase = service.Sign(fields, null);

        // Assert
        withPassphrase.ShouldNotBe(withoutPassphrase);
    }
}
