// Extra search synonyms so filtering by "azul" finds the "blue" family regardless of active language.
export const COLOR_FAMILY_ALIASES: Readonly<Record<string, readonly string[]>> = {
  blue: ['azul'],
  red: ['vermelho'],
  green: ['verde'],
  yellow: ['amarelo'],
  purple: ['roxo', 'lilas', 'lilás'],
  pink: ['rosa'],
  orange: ['laranja'],
  neutral: ['neutro', 'bege', 'cinza'],
};
