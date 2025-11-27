/**
 * Shared constants for fractions
 */
export const FRACTION_IDS = {
  HEGEMONIA: 'hegemonia_titanum',
  SHIMURA: 'shimura_incorporated',
  PROTEKTORAT: 'protektorat_pogranicza',
};

export const FRACTION_NAMES = {
  [FRACTION_IDS.HEGEMONIA]: 'Hegemonia Titanum',
  [FRACTION_IDS.SHIMURA]: 'Shimura Incorporated',
  [FRACTION_IDS.PROTEKTORAT]: 'Protektorat Pogranicza',
};

export const FRACTIONS_LIST = [
  { id: FRACTION_IDS.HEGEMONIA, name: FRACTION_NAMES[FRACTION_IDS.HEGEMONIA] },
  { id: FRACTION_IDS.SHIMURA, name: FRACTION_NAMES[FRACTION_IDS.SHIMURA] },
  { id: FRACTION_IDS.PROTEKTORAT, name: FRACTION_NAMES[FRACTION_IDS.PROTEKTORAT] },
];

