"use client";

import StatusBadge from "@/features/status/status.badge";
import { Button } from "@/shared/components/ui/button";
import {
  Trash2,
  Building2,
  ChevronRight,
  Video,
  PlayCircle,
} from "lucide-react";
import { useState } from "react";
import Link from "next/link";
import { Department } from "../types";
import { DeleteDepartmentDialog } from "@/features/departments/model/delete-department-dialog";
import { FileUploadDialog } from "@/entities/file/ui/file-upload-dialog";
import { useUpdateDepartmentVideo } from "@/features/departments/model/use-update-department-video";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import {
  VideoPlayer,
  VideoPlayerContent,
  VideoPlayerControlBar,
  VideoPlayerMuteButton,
  VideoPlayerPlayButton,
  VideoPlayerSeekBackwardButton,
  VideoPlayerSeekForwardButton,
  VideoPlayerTimeDisplay,
  VideoPlayerTimeRange,
  VideoPlayerVolumeRange,
} from "@/components/kibo-ui/video-player";

export default function DepartmentCard({
  department,
}: {
  department: Department;
}) {
  const [openDelete, setOpenDelete] = useState(false);
  const [openVideoUpload, setOpenVideoUpload] = useState(false);
  const [openVideoPlayer, setOpenVideoPlayer] = useState(false);

  const { updateDepartmentVideo } = useUpdateDepartmentVideo();

  // Проверяем наличие видео и URL
  const hasVideo = !!department.video?.url;

  return (
    <div className="flex flex-col justify-between rounded-xl border border-border bg-card p-6 text-card-foreground shadow-sm transition-all hover:shadow-md hover:bg-accent/5">
      <div>
        <div className="mb-4 flex items-start justify-between">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-primary/10 rounded-lg text-primary">
              <Building2 className="h-5 w-5" />
            </div>
            <div>
              <h3
                className="text-lg font-semibold tracking-tight line-clamp-1"
                title={department.name}
              >
                {department.name}
              </h3>
              <span className="text-xs font-mono text-muted-foreground uppercase">
                {department.identifier}
              </span>
            </div>
          </div>
          <div className="flex items-center gap-2">
            {hasVideo && (
              <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8 text-primary hover:bg-primary/10"
                onClick={() => setOpenVideoPlayer(true)}
              >
                <PlayCircle className="h-4 w-4" />
              </Button>
            )}

            <Button
              variant="ghost"
              size="icon"
              className="h-8 w-8 text-muted-foreground hover:text-primary"
              onClick={() => setOpenVideoUpload(true)}
            >
              <Video className="h-4 w-4" />
            </Button>

            <StatusBadge isActive={department.isActive} />

            <Button
              variant="ghost"
              size="icon"
              className="h-8 w-8 text-destructive hover:bg-destructive hover:text-destructive-foreground"
              onClick={() => setOpenDelete(true)}
            >
              <Trash2 className="h-4 w-4" />
            </Button>
          </div>
        </div>

        <div className="space-y-3 text-sm">
          <div className="bg-muted/50 p-2 rounded text-xs font-mono text-muted-foreground break-all">
            {department.path}
          </div>
          <div className="space-y-1 text-muted-foreground">
            <p className="flex items-center gap-2">
              <span className="font-medium">Уровень:</span>
              <span className="text-foreground">{department.depth}</span>
            </p>
          </div>
        </div>
      </div>

      <div className="mt-6 border-t border-border pt-4">
        <Button asChild className="w-full justify-between" variant="secondary">
          <Link href={`/departments/${department.id}`}>
            Подробнее
            <ChevronRight className="h-4 w-4" />
          </Link>
        </Button>
      </div>

      <DeleteDepartmentDialog
        open={openDelete}
        onOpenChange={setOpenDelete}
        department={department}
      />

      <FileUploadDialog
        open={openVideoUpload}
        onOpenChange={setOpenVideoUpload}
        ownerId={department.id}
        ownerType="department"
        assetType="video"
        title="Загрузить видеоматериалы"
        description={`Выберите файл для подразделения: ${department.name}`}
        onSuccess={async (mediaAssetId) => {
          updateDepartmentVideo({
            departmentId: department.id,
            videoId: mediaAssetId,
          });
        }}
      />

      <Dialog open={openVideoPlayer} onOpenChange={setOpenVideoPlayer}>
        <DialogContent className="max-w-4xl p-0 overflow-hidden bg-black">
          <DialogHeader className="p-4 bg-card border-b">
            <DialogTitle>{department.name} — Видеоматериалы</DialogTitle>
          </DialogHeader>
          <div className="aspect-video w-full">
            {department.video?.url && (
              <VideoPlayer>
                <VideoPlayerContent
                  src={department.video.url}
                  crossOrigin=""
                  preload="auto"
                  muted
                  slot="media"
                  className="w-full h-full"
                />
                <VideoPlayerControlBar>
                  <VideoPlayerPlayButton />
                  <VideoPlayerSeekBackwardButton />
                  <VideoPlayerSeekForwardButton />
                  <VideoPlayerTimeRange />
                  <VideoPlayerTimeDisplay showDuration />
                  <VideoPlayerMuteButton />
                  <VideoPlayerVolumeRange />
                </VideoPlayerControlBar>
              </VideoPlayer>
            )}
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
