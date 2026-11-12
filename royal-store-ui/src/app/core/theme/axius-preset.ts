import Aura from '@primeuix/themes/aura';
import { definePreset } from '@primeuix/themes';

/**
 * Axius Store design tokens, applied on top of PrimeNG's Aura preset.
 *
 * Why this file exists: stock Aura picks `primary.500` (#10B981) as the light
 * interactive colour. White label text on that fill measures ~2.6:1, below the
 * 4.5:1 minimum, so every primary button in the app shipped under-contrast.
 * Moving the light scheme to `primary.700` (#047857, ~5.5:1 on white) fixes the
 * whole app at the token level and lines the product up with the marketing site,
 * which uses the same emerald.
 *
 * Everything else here is the same idea: one place where contrast, focus,
 * density and radius are decided, instead of per-component overrides.
 */
export const AxiusPreset = definePreset(Aura, {
  semantic: {
    // Aura ships a 1px focus ring; 2px is the floor for a ring that stays
    // visible against both surface and primary fills.
    focusRing: {
      width: '2px',
      style: 'solid',
      color: '{primary.color}',
      offset: '2px',
      shadow: 'none',
    },

    // Taller fields: 0.625rem padding puts inputs and buttons at ~44px, the
    // minimum comfortable pointer/touch target.
    formField: {
      paddingX: '0.875rem',
      paddingY: '0.625rem',
      borderRadius: '10px',
    },

    content: {
      borderRadius: '12px',
    },

    navigation: {
      item: {
        padding: '0.625rem 0.75rem',
        borderRadius: '8px',
        gap: '0.625rem',
      },
    },

    colorScheme: {
      light: {
        primary: {
          color: '{primary.700}',
          contrastColor: '#ffffff',
          hoverColor: '{primary.800}',
          activeColor: '{primary.800}',
        },
        highlight: {
          background: '{primary.50}',
          focusBackground: '{primary.100}',
          color: '{primary.800}',
          focusColor: '{primary.800}',
        },
        // Body copy on slate.800 instead of slate.700: still soft, but clears
        // 4.5:1 against every surface tint used in the app.
        text: {
          color: '{surface.800}',
          hoverColor: '{surface.900}',
          mutedColor: '{surface.500}',
          hoverMutedColor: '{surface.600}',
        },
        navigation: {
          item: {
            activeBackground: '{primary.50}',
            activeColor: '{primary.800}',
            focusBackground: '{surface.100}',
            icon: {
              activeColor: '{primary.700}',
            },
          },
        },
      },

      dark: {
        primary: {
          color: '{primary.400}',
          contrastColor: '{surface.950}',
          hoverColor: '{primary.300}',
          activeColor: '{primary.300}',
        },
        // Aura mixes neutrals across themes (slate in light, zinc in dark).
        // One neutral family keeps borders, muted text and surfaces reading the
        // same in both modes.
        surface: {
          0: '#ffffff',
          50: '{slate.50}',
          100: '{slate.100}',
          200: '{slate.200}',
          300: '{slate.300}',
          400: '{slate.400}',
          500: '{slate.500}',
          600: '{slate.600}',
          700: '{slate.700}',
          800: '{slate.800}',
          900: '{slate.900}',
          950: '{slate.950}',
        },
        highlight: {
          background: 'color-mix(in srgb, {primary.400}, transparent 84%)',
          focusBackground: 'color-mix(in srgb, {primary.400}, transparent 76%)',
          color: '{primary.300}',
          focusColor: '{primary.200}',
        },
        text: {
          color: '{surface.0}',
          hoverColor: '{surface.0}',
          mutedColor: '{surface.400}',
          hoverMutedColor: '{surface.300}',
        },
        navigation: {
          item: {
            activeBackground: 'color-mix(in srgb, {primary.400}, transparent 86%)',
            activeColor: '{primary.300}',
            focusBackground: '{surface.800}',
            icon: {
              activeColor: '{primary.300}',
            },
          },
        },
      },
    },
  },
});
