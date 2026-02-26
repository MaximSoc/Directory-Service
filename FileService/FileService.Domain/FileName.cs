using CSharpFunctionalExtensions;
using SharedKernel;

namespace FileService.Domain
{
    public sealed record FileName
    {
        public string Name { get; }

        public string Extension { get; }

        private FileName(string name, string extension)
        {
            Name = name;
            Extension = extension;
        }

        public static Result<FileName, Error> Create(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return GeneralErrors.ValueIsRequired("FileName");
            }

            int lasDot = fileName.LastIndexOf('.');
            if (lasDot == -1 || lasDot == fileName.Length - 1)
            {
                return GeneralErrors.ValueIsInvalid("File must have extension");
            }

            string extension = fileName[(lasDot + 1)..].ToLowerInvariant();


            return new FileName(fileName, extension);
        }
    }
}
