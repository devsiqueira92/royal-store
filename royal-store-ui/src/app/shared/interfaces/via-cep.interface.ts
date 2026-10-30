export interface ViaCepAddress {
  cep: string;
  logradouro: string;
  complemento: string;
  unidade: string;
  bairro: string;
  localidade: string;
  uf: string;
  estado: string;
  regiao: string;
  ibge: string;
  gia: string;
  ddd: string;
  siafi: string;
}

/** Unknown zip codes still come back as 200, with this body instead of an address. */
export interface ViaCepNotFound {
  erro: boolean | string;
}

export type ViaCepResponse = ViaCepAddress | ViaCepNotFound;
