/**
 * Minimalist hero over an ambient brand field. Deliberately not a photograph:
 * there is no editorially-chosen hero image in the data, and a listing photo
 * pulled at random would misrepresent whichever listing it belonged to.
 */
export function BrowseHero() {
  return (
    <section className="relative overflow-hidden border-b border-hero-line bg-hero-gradient">
      <div className="relative mx-auto max-w-[1400px] px-4 pb-20 pt-14 sm:px-6 sm:pb-24 sm:pt-20">
        <p className="text-2xs font-semibold uppercase tracking-[0.18em] text-white/60">
          Township homes · straight from the owners
        </p>
        <h1 className="mt-3 max-w-2xl font-display text-4xl font-bold leading-[1.08] tracking-tight text-white sm:text-5xl">
          Find a place that feels like home.
        </h1>
        <p className="mt-4 max-w-xl text-base leading-relaxed text-white/70">
          Backrooms, cottages and houses listed directly by owners — no agents, no commission.
        </p>
      </div>
    </section>
  );
}
