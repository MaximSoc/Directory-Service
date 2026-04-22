namespace FileService.Domain;

public abstract partial class MediaAsset
{
    public enum MediaStatus
    {
        UPLOADING,
        UPLOADED,
        PROCESSING,
        READY,
        FAILED,
        DELETED,
    }
}
