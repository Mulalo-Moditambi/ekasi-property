import { useCallback, useEffect, useState } from 'react';

export type Theme = 'light' | 'dark';

const KEY = 'ekasi.theme';

function preferredTheme(): Theme {
  const stored = localStorage.getItem(KEY);
  if (stored === 'light' || stored === 'dark') {
    return stored;
  }

  // The cream light theme is the brand, so it is the default for everyone —
  // deliberately not following `prefers-color-scheme`, which was showing the
  // dark theme to anyone whose OS is set dark. Dark stays available via the toggle.
  return 'light';
}

/** Class-driven theming: cream by default, dark only if the visitor asks for it. */
export function useTheme(): { theme: Theme; toggle: () => void } {
  const [theme, setTheme] = useState<Theme>(preferredTheme);

  useEffect(() => {
    document.documentElement.classList.toggle('dark', theme === 'dark');
    document.documentElement.style.colorScheme = theme;
  }, [theme]);

  const toggle = useCallback(() => {
    setTheme((current) => {
      const next = current === 'dark' ? 'light' : 'dark';
      localStorage.setItem(KEY, next);
      return next;
    });
  }, []);

  return { theme, toggle };
}
