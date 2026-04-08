"use client";

import { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from "@/shared/components/ui/dialog";
import {
  Dropzone,
  DropzoneContent,
  DropzoneEmptyState,
} from "@/components/kibo-ui/dropzone";
import { useFileUpload } from "../model/use-file-upload";
import { AssetType, OwnerType } from "../types";
import { getValidatorConfig } from "../lib/validators";
import { UploadingState } from "./uploading-state";
import { CompleteState } from "./completed-state";
import { ErrorState } from "./error-state";

type Props = {
  ownerId: string;
  ownerType: OwnerType;
  assetType: AssetType;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title?: string;
  description?: string;
};

export function FileUploadDialog({
  ownerId,
  ownerType,
  assetType,
  open,
  onOpenChange,
  title,
  description,
}: Props) {
  const [files, setFiles] = useState<File[]>([]);
  const config = getValidatorConfig(assetType);

  const {
    upload,
    abort,
    uploadState,
    isIdle,
    isUploading,
    isCompleted,
    isFailed,
    error,
  } = useFileUpload({
    ownerId,
    ownerType,
    assetType,
    onAbort: () => setFiles([]),
  });

  const handleOpenChange = (newOpen: boolean) => {
    if (!newOpen) setFiles([]);
    onOpenChange(newOpen);
  };

  const handleDrop = async (acceptedFiles: File[]) => {
    if (acceptedFiles.length > 0) {
      setFiles(acceptedFiles);
      await upload(acceptedFiles[0]);
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="sm:max-w-125" key={open ? "open" : "closed"}>
        <DialogHeader>
          <DialogTitle>
            {title || `Загрузить ${config.label.toLowerCase()}`}
          </DialogTitle>
          {description && <DialogDescription>{description}</DialogDescription>}
        </DialogHeader>

        <div className="py-4">
          {isIdle && (
            <Dropzone
              onDrop={handleDrop}
              src={files}
              accept={config.allowedTypes.reduce(
                (acc, type, index) => ({
                  ...acc,
                  [type]: [config.allowedExtensions[index]],
                }),
                {}
              )}
              maxFiles={1}
            >
              <DropzoneEmptyState>
                <div className="text-center">
                  <p className="font-medium text-sm">
                    Выберите {config.label.toLowerCase()}
                  </p>
                  <p className="text-xs text-muted-foreground">
                    {config.description}
                  </p>
                </div>
              </DropzoneEmptyState>
              <DropzoneContent />
            </Dropzone>
          )}

          {isUploading && (
            <UploadingState
              fileName={uploadState.fileName}
              progress={uploadState.progress}
              onAbort={abort}
            />
          )}

          {isCompleted && <CompleteState />}

          {isFailed && <ErrorState error={error} />}
        </div>
      </DialogContent>
    </Dialog>
  );
}
