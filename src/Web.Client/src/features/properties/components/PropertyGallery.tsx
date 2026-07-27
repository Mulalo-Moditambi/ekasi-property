import { useState } from 'react';
import { CameraIcon, ChevronIcon } from '../../../shared/components/icons';
import { PhotoLightbox } from './PhotoLightbox';

type GalleryLayout = 'mosaic' | 'theater';

const LAYOUT_KEY = 'ekasi.gallery';

interface PropertyGalleryProps {
  urls: string[];
  alt: string;
}

/**
 * Detail-page gallery with two switchable layouts (persisted per browser):
 * - mosaic:  one large + two small tiles, "+N more" overflow badge
 * - theater: one full-width stage with prev/next and a thumbnail strip
 * Clicking any photo opens the lightbox at that photo; all photos cycle from there.
 */
export function PropertyGallery({ urls, alt }: PropertyGalleryProps) {
  const [layout, setLayout] = useState<GalleryLayout>(() =>
    localStorage.getItem(LAYOUT_KEY) === 'theater' ? 'theater' : 'mosaic',
  );
  const [stageIndex, setStageIndex] = useState(0);
  const [lightboxIndex, setLightboxIndex] = useState<number | null>(null);

  if (urls.length === 0) {
    return (
      <div className="photo-grid-placeholder" aria-label="No photos yet">
        <span>🏠</span>
      </div>
    );
  }

  function switchLayout(next: GalleryLayout) {
    setLayout(next);
    localStorage.setItem(LAYOUT_KEY, next);
  }

  const showPrevious = () => setStageIndex((i) => (i - 1 + urls.length) % urls.length);
  const showNext = () => setStageIndex((i) => (i + 1) % urls.length);

  const mosaicTiles = urls.slice(0, 3);
  const overflowCount = urls.length - mosaicTiles.length;

  return (
    <div className="gallery">
      <div className="gallery-header">
        <span className="muted photo-summary">
          <CameraIcon />
          {urls.length} photo{urls.length === 1 ? '' : 's'}
        </span>
        {urls.length > 1 && (
          <div className="view-toggle gallery-toggle" role="group" aria-label="Gallery layout">
            <button
              type="button"
              className={layout === 'mosaic' ? 'active' : ''}
              onClick={() => switchLayout('mosaic')}
            >
              ▦ Mosaic
            </button>
            <button
              type="button"
              className={layout === 'theater' ? 'active' : ''}
              onClick={() => switchLayout('theater')}
            >
              ▭ Theater
            </button>
          </div>
        )}
      </div>

      {layout === 'mosaic' ? (
        <div className={`photo-grid count-${mosaicTiles.length}`}>
          {mosaicTiles.map((url, index) => (
            <button
              key={url}
              type="button"
              className="photo-tile"
              aria-label={`Open photo ${index + 1} of ${urls.length}`}
              onClick={() => setLightboxIndex(index)}
            >
              <img src={url} alt={`${alt} — photo ${index + 1}`} loading={index === 0 ? 'eager' : 'lazy'} />
              {overflowCount > 0 && index === mosaicTiles.length - 1 && (
                <span className="photo-more">+{overflowCount} more</span>
              )}
            </button>
          ))}
        </div>
      ) : (
        <div className="photo-theater">
          <div className="theater-frame">
            <button
              type="button"
              className="theater-stage"
              aria-label={`Open photo ${stageIndex + 1} of ${urls.length}`}
              onClick={() => setLightboxIndex(stageIndex)}
            >
              <img src={urls[stageIndex]} alt={`${alt} — photo ${stageIndex + 1}`} />
            </button>
            {urls.length > 1 && (
              <>
                <button type="button" className="theater-nav prev" aria-label="Previous photo" onClick={showPrevious}>
                  <ChevronIcon direction="left" size={18} />
                </button>
                <button type="button" className="theater-nav next" aria-label="Next photo" onClick={showNext}>
                  <ChevronIcon direction="right" size={18} />
                </button>
                <span className="theater-count">
                  {stageIndex + 1} / {urls.length}
                </span>
              </>
            )}
          </div>
          {urls.length > 1 && (
            <div className="theater-thumbs" role="group" aria-label="Photo thumbnails">
              {urls.map((url, index) => (
                <button
                  key={url}
                  type="button"
                  className={index === stageIndex ? 'thumb active' : 'thumb'}
                  aria-label={`Show photo ${index + 1}`}
                  aria-current={index === stageIndex}
                  onClick={() => setStageIndex(index)}
                >
                  <img src={url} alt="" loading="lazy" />
                </button>
              ))}
            </div>
          )}
        </div>
      )}

      {lightboxIndex !== null && (
        <PhotoLightbox urls={urls} startIndex={lightboxIndex} alt={alt} onClose={() => setLightboxIndex(null)} />
      )}
    </div>
  );
}
