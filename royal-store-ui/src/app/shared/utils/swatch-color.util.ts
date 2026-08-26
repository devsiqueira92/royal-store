export function getSwatchColor(value: string | null | undefined): string {
  if (!value) {
    return 'var(--p-surface-300)';
  }

  let hash = 0;
  for (const char of value) {
    hash = char.charCodeAt(0) + ((hash << 5) - hash);
  }

  return `hsl(${Math.abs(hash) % 360}, 45%, 55%)`;
}
