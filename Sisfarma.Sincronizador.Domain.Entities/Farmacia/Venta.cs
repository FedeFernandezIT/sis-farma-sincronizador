using System;
using System.Collections;
using System.Collections.Generic;

namespace Sisfarma.Sincronizador.Domain.Entities.Farmacia
{    
    public class Venta
    {
        public long Id { get; set; }

        public DateTime FechaHora { get; set; }

        public int Puesto { get; set; }

        public long ClienteId { get; set; }

        public string ClienteCodigo { get; set; }

        public Cliente Cliente { get; set; }

        public long VendedorId { get; set; }

        public string VendedorCodigo { get; set; }

        public string VendedorNombre { get; set; }

        public decimal TotalDescuento { get; set; }

        public decimal TotalBruto { get; set; }

        public string Tipo { get; set; }

        public decimal Importe { get; set; }

        //public string Usuario { get; set; }

        //public string Maquina { get; set; }

        //public string TipoVenta { get; set; }

        //public double TotalBase { get; set; }

        //public double TotalCuota { get; set; }

        //public double TotalRecargo { get; set; }

        //public double TotalVentaBruta { get; set; }

        //public double? DescuentoLinea { get; set; }

        //public double? DescuentoOpera { get; set; }

        //public double TotalVenta { get; set; }

        //public string RecetaPendiente { get; set; }

        //public string Facturada { get; set; }

        //public string NumeroDoc { get; set; }

        //public int? IdContador { get; set; }

        //public int? Empresa { get; set; }

        public Ticket Ticket { get; set; }

        public ICollection<VentaDetalle> Detalle { get; set; }

        public Venta() 
            => Detalle = new HashSet<VentaDetalle>();

        public bool HasDetalle() => Detalle != null && Detalle.Count > 0;
        
        public bool HasCliente() => this.Cliente != null;
    }

    public class VentaDetalle
    {
        public long Id { get; set; }

        public long VentaId { get; set; }

        public int Linea { get; set; }

        public string Receta { get; set; }

        public string Codigo { get; set; }

        public string Descripcion { get; set; }

        public int Cantidad { get; set; }

        public decimal Importe { get; set; }

        public decimal PVP { get; set; }

        public decimal Descuento { get; set; }        

        //public string TipoAportacion { get; set; }

        //public double ImporteBruto { get; set; }

        //public double? DescuentoLinea { get; set; }

        //public double? DescuentoOpera { get; set; }

        //public double ImporteNeto { get; set; }

        //public string TipoLinea { get; set; }

        //public string RecetaPendiente { get; set; }

        //public bool Facturada { get; set; }

        //public int? Factura { get; set; }

        public Farmaco Farmaco { get; set; }

        public bool HasFarmaco() => Farmaco != null;        
    }

    public class Ticket
    {
        public long Numero { get; set; }

        public string Serie { get; set; }
    }
}
