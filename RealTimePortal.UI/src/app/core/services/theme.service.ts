import {
  DOCUMENT,
  isPlatformBrowser
} from '@angular/common';

import {
  Inject,
  Injectable,
  PLATFORM_ID,
  signal
} from '@angular/core';

export type ThemeMode = 'light' | 'dark';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {

  private readonly storageKey =
    'realTimePortalTheme';

  readonly theme =
    signal<ThemeMode>('light');


  constructor(
    @Inject(DOCUMENT)
    private readonly document: Document,

    @Inject(PLATFORM_ID)
    private readonly platformId: object
  ) {

    // Read saved theme AFTER Angular injections
    // are available.
    this.theme.set(
      this.getInitialTheme()
    );

    this.applyTheme(
      this.theme()
    );
  }


  // ============================================================
  // TOGGLE THEME
  // ============================================================

  toggleTheme(): void {

    const newTheme =
      this.theme() === 'light'
        ? 'dark'
        : 'light';

    this.setTheme(
      newTheme
    );
  }


  // ============================================================
  // SET THEME
  // ============================================================

  setTheme(
    theme: ThemeMode
  ): void {

    this.theme.set(
      theme
    );

    if (
      isPlatformBrowser(
        this.platformId
      )
    ) {

      localStorage.setItem(
        this.storageKey,
        theme
      );
    }

    this.applyTheme(
      theme
    );
  }


  // ============================================================
  // APPLY CURRENT THEME
  // ============================================================

  applyCurrentTheme(): void {

    this.applyTheme(
      this.theme()
    );
  }


  // ============================================================
  // APPLY THEME
  // ============================================================

  private applyTheme(
    theme: ThemeMode
  ): void {

    const html =
      this.document.documentElement;

    const body =
      this.document.body;


    html.classList.remove(
      'light-theme',
      'dark-theme'
    );

    body.classList.remove(
      'light-theme',
      'dark-theme'
    );


    html.classList.add(
      `${theme}-theme`
    );

    body.classList.add(
      `${theme}-theme`
    );


    html.setAttribute(
      'data-theme',
      theme
    );


    html.style.colorScheme =
      theme;
  }


  // ============================================================
  // GET INITIAL THEME
  // ============================================================

  private getInitialTheme(): ThemeMode {

    if (
      isPlatformBrowser(
        this.platformId
      )
    ) {

      try {

        const savedTheme =
          localStorage.getItem(
            this.storageKey
          );


        if (
          savedTheme === 'light' ||
          savedTheme === 'dark'
        ) {

          return savedTheme;
        }


        const cookieMatch =
          document.cookie.match(
            new RegExp(
              `(?:^|;\\s*)${this.storageKey}=(dark|light)`
            )
          );


        if (
          cookieMatch?.[1] === 'dark' ||
          cookieMatch?.[1] === 'light'
        ) {

         return cookieMatch[1] === 'dark'
            ? 'dark'
            : 'light';
        }

      }
      catch {
        // Fall back to light theme.
      }
    }


    return 'light';
  }
}