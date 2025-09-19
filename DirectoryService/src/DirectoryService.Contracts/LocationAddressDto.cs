namespace DirectoryService.Contracts
{
    public record LocationAddressDto(
        string Country,
        string Region,
        string City,
        string PostalCode,
        string Street,
        string ApartamentNumber);
}
