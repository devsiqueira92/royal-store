namespace RoyalStore.Domain.Catalog;

/// <summary>
/// CST de Origem da Mercadoria (codes 0-8) used on Brazilian fiscal documents.
/// The persisted column and the API wire representation remain the integer code.
/// </summary>
public enum ProductOrigin
{
    Nacional = 0,
    EstrangeiraImportacaoDireta = 1,
    EstrangeiraMercadoInterno = 2,
    NacionalConteudoImportacaoSuperior40 = 3,
    NacionalProcessoProdutivoBasico = 4,
    NacionalConteudoImportacaoInferiorOuIgual40 = 5,
    EstrangeiraImportacaoDiretaSemSimilar = 6,
    EstrangeiraMercadoInternoSemSimilar = 7,
    NacionalConteudoImportacaoSuperior70 = 8
}
