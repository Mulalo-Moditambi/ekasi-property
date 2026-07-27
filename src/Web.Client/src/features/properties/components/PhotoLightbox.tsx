import { useCallback, useEffect, useState } from 'react';
import { ChevronIcon, CloseIcon } from '../../../shared/components/icons';

interface PhotoLightboxProps {
  urls: string[];
  startIndex: number;
  alt: string;
  onClose: () => void;
}

/** Full-screen photo viewer. Esc closes, arrow keys / side buttons cycle through every photo. */
export function PhotoLightbox({ urls, startIndex, alt, onClose }: PhotoLightboxProps) {
  const [index, setIndex] = useState(startIndex);

  const prev = useCallback(() => setIndex((i) => (i - 1 + urls.length) % urls.length), [urls.length]);
  const next = useCallback(() => setIndex((i) => (i + 1) % urls.length), [urls.length]);

  useEffect(() => {
    function onKey(event: KeyboardEvent) {
      if (event.key === 'Escape') onClose();
      if (event.key === 'ArrowLeft') prev();
      if (event.key === 'ArrowRight') next();
    }

    window.addEventListener('keydown', onKey);
    document.body.style.overflow = 'hidden';

    return () => {
      window.removeEventListener('keydown', onKey);
      document.body.style.overflow = '';
    };
  }, [onClose, prev, next]);

  return (
    <div className="lightbox" role="dialog" aria-modal="true" aria-label={`${alt} — photo viewer`} onClick={onClose}>
      <button type="button" className="lightbox-close" aria-label="Close photo viewer" onClick={onClose}>
        <CloseIcon />
      </button>
      <span className="lightbox-count">
        {index + 1} / {urls.length}
      </span>
      {urls.length > 1 && (
        <button
          type="button"
          className="lightbox-nav prev"
          aria-label="Previous photo"
          onClick={(e) => {
            e.stopPropagation();
            prev();
          }}
        >
          <ChevronIcon direction="left" size={22} />
        </button>
      )}
      <img src={urls[index]} alt={`${alt} — photo ${index + 1} of ${urls.length}`} onClick={(e) => e.stopPropagation()} />
      {urls.length > 1 && (
        <button
          type="button"
          className="lightbox-nav next"
          aria-label="Next photo"
          onClick={(e) => {
            e.stopPropagation();
            next();
          }}
        >
          <ChevronIcon direction="right" size={22} />
        </button>
      )}
    </div>
  );
}
