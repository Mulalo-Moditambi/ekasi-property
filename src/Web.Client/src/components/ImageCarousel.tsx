import { useRef } from 'react';
import type { MouseEvent } from 'react';

interface ImageCarouselProps {
  urls: string[];
  alt: string;
}

export function ImageCarousel({ urls, alt }: ImageCarouselProps) {
  const trackRef = useRef<HTMLDivElement>(null);

  function scroll(event: MouseEvent, direction: -1 | 1) {
    // The carousel often lives inside a Link — don't navigate.
    event.preventDefault();
    event.stopPropagation();

    const track = trackRef.current;
    if (track) {
      track.scrollBy({ left: direction * track.clientWidth, behavior: 'smooth' });
    }
  }

  if (urls.length === 0) {
    return (
      <div className="carousel placeholder" aria-label="No photos yet">
        <span>🏠</span>
      </div>
    );
  }

  return (
    <div className="carousel">
      <div className="carousel-track" ref={trackRef}>
        {urls.map((url, index) => (
          <img key={url} src={url} alt={`${alt} — photo ${index + 1}`} loading="lazy" />
        ))}
      </div>
      {urls.length > 1 && (
        <>
          <button type="button" className="carousel-nav prev" aria-label="Previous photo" onClick={(e) => scroll(e, -1)}>
            ‹
          </button>
          <button type="button" className="carousel-nav next" aria-label="Next photo" onClick={(e) => scroll(e, 1)}>
            ›
          </button>
          <span className="carousel-count">{urls.length} photos</span>
        </>
      )}
    </div>
  );
}
