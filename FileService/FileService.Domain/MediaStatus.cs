namespace FileService.Domain
{
    public abstract partial class MediaAsset
    {
        public enum MediaStatus
        {
            UPLOADING,
            UPLOADED,
            READY,
            FAILED,
            DELETED,
        }
    }
}
