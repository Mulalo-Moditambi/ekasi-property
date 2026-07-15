using SharedKernel;

namespace Domain.Inquiries;

public static class InquiryErrors
{
    public static Error NotFound(Guid inquiryId) => Error.NotFound(
        "Inquiries.NotFound",
        $"The inquiry with the Id = '{inquiryId}' was not found");
}
