namespace BasculasPG.Models.Dapper
{
    public class PesoRequest
    {
        public List<InsertDetallePeso> pesos { get; set; }
        public InsertInfoGuia guiaData { get; set; }
        public InfoInterna InfoInterna { get; set; }
        public InfoPeso InfoPeso { get; set; }
    }
}
