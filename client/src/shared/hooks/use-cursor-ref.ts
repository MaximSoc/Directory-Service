import { useCallback, useRef } from "react";

interface UseCursorRefProps {
  hasNextPage: boolean;
  isFetchingNextPage: boolean;
  fetchNextPage: () => void;
  threshold?: number;
}

export function useCursorRef({
  hasNextPage,
  isFetchingNextPage,
  fetchNextPage,
  threshold = 0.5,
}: UseCursorRefProps) {
  const observerRef = useRef<IntersectionObserver | null>(null);

  const cursorRef = useCallback(
    (node: HTMLElement | null) => {
      if (observerRef.current) {
        observerRef.current.disconnect();
      }

      if (!node) return;

      observerRef.current = new IntersectionObserver(
        (entries) => {
          if (entries[0].isIntersecting && hasNextPage && !isFetchingNextPage) {
            fetchNextPage();
          }
        },
        { threshold }
      );

      observerRef.current.observe(node);
    },
    [hasNextPage, isFetchingNextPage, fetchNextPage, threshold]
  );

  return cursorRef;
}
