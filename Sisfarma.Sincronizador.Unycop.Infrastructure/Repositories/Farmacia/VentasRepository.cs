﻿using Sisfarma.Sincronizador.Core.Config;
using Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia;
using Sisfarma.Sincronizador.Domain.Entities.Farmacia;
using Sisfarma.Sincronizador.Unycop.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia
{
    public class VentasRepository : FarmaciaRepository, IVentasRepository
    {
        private readonly IClientesRepository _clientesRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IVendedoresRepository _vendedoresRepository;
        private readonly IFarmacoRepository _farmacoRepository;
        private readonly ICodigoBarraRepository _barraRepository;
        private readonly IProveedorRepository _proveedorRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly IFamiliaRepository _familiaRepository;
        private readonly ILaboratorioRepository _laboratorioRepository;

        private readonly decimal _factorCentecimal = 0.01m;
        
        public VentasRepository(LocalConfig config, 
            IClientesRepository clientesRepository,
            ITicketRepository ticketRepository, 
            IVendedoresRepository vendedoresRepository,
            IFarmacoRepository farmacoRepository,
            ICodigoBarraRepository barraRepository,
            IProveedorRepository proveedorRepository,
            ICategoriaRepository categoriaRepository,
            IFamiliaRepository familiaRepository,
            ILaboratorioRepository laboratorioRepository) : base(config)
        {
            _clientesRepository = clientesRepository ?? throw new ArgumentNullException(nameof(clientesRepository));
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _farmacoRepository = farmacoRepository ?? throw new ArgumentNullException(nameof(farmacoRepository));
            _barraRepository = barraRepository ?? throw new ArgumentNullException(nameof(barraRepository));
            _proveedorRepository = proveedorRepository ?? throw new ArgumentNullException(nameof(proveedorRepository));
            _categoriaRepository = categoriaRepository ?? throw new ArgumentNullException(nameof(categoriaRepository));
            _familiaRepository = familiaRepository ?? throw new ArgumentNullException(nameof(familiaRepository));
            _laboratorioRepository = laboratorioRepository ?? throw new ArgumentNullException(nameof(laboratorioRepository));
            _vendedoresRepository = vendedoresRepository ?? throw new ArgumentNullException(nameof(vendedoresRepository));
        }

        public VentasRepository(
            IClientesRepository clientesRepository,
            ITicketRepository ticketRepository,
            IVendedoresRepository vendedoresRepository,
            IFarmacoRepository farmacoRepository,
            ICodigoBarraRepository barraRepository,
            IProveedorRepository proveedorRepository,
            ICategoriaRepository categoriaRepository,
            IFamiliaRepository familiaRepository,
            ILaboratorioRepository laboratorioRepository)
        {
            _clientesRepository = clientesRepository ?? throw new ArgumentNullException(nameof(clientesRepository));
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _farmacoRepository = farmacoRepository ?? throw new ArgumentNullException(nameof(farmacoRepository));
            _barraRepository = barraRepository ?? throw new ArgumentNullException(nameof(barraRepository));
            _proveedorRepository = proveedorRepository ?? throw new ArgumentNullException(nameof(proveedorRepository));
            _categoriaRepository = categoriaRepository ?? throw new ArgumentNullException(nameof(categoriaRepository));
            _familiaRepository = familiaRepository ?? throw new ArgumentNullException(nameof(familiaRepository));
            _laboratorioRepository = laboratorioRepository ?? throw new ArgumentNullException(nameof(laboratorioRepository));
            _vendedoresRepository = vendedoresRepository ?? throw new ArgumentNullException(nameof(vendedoresRepository));
        }        

        public Venta GetOneOrDefaultById(long id)
        {
            var year = int.Parse($"{id}".Substring(0, 4));
            var ventaId = int.Parse($"{id}".Substring(4));
            
            DTO.Venta ventaAccess;
            try
            {
                using (var db = FarmaciaContext.VentasByYear(year))
                {
                    var sql = @"SELECT ID_VENTA as Id, Fecha, NPuesto as Puesto, Cliente, Vendedor, Descuento, Pago, Tipo, Importe FROM ventas WHERE ID_VENTA = @id";
                    ventaAccess = db.Database.SqlQuery<DTO.Venta>(sql,
                        new OleDbParameter("id", ventaId))
                        .FirstOrDefault();
                }
            }
            catch (FarmaciaContextException)
            {
                ventaAccess = null;
            }

            if (ventaAccess == null)
                return null;

            var venta = new Venta
            {
                Id = ventaAccess.Id,
                Tipo = ventaAccess.Tipo.ToString(),
                FechaHora = ventaAccess.Fecha,
                Puesto = ventaAccess.Puesto,
                ClienteId = ventaAccess.Cliente,
                VendedorId = ventaAccess.Vendedor,
                TotalDescuento = ventaAccess.Descuento * _factorCentecimal,
                TotalBruto = ventaAccess.Pago * _factorCentecimal,
                Importe = ventaAccess.Importe * _factorCentecimal,
            };


            if (ventaAccess.Cliente > 0)
                venta.Cliente = _clientesRepository.GetOneOrDefaultById(ventaAccess.Cliente);

            var ticket = _ticketRepository.GetOneOrdefaultByVentaId(ventaAccess.Id, year);
            if (ticket != null)
            {
                venta.Ticket = new Ticket
                {
                    Numero = ticket.Numero,
                    Serie = ticket.Serie
                };
            }

            venta.VendedorNombre = _vendedoresRepository.GetOneOrDefaultById(ventaAccess.Vendedor)?.Nombre;
            venta.Detalle = GetDetalleDeVentaByVentaId(year, ventaAccess.Id);

            return venta;
        }

        public List<Venta> GetAllByIdGreaterOrEqual(int year, long value)
        {
            // Access no handlea long
            var valueInteger = (int)value;
            List<DTO.Venta> ventasAccess;

            try
            {
                using (var db = FarmaciaContext.VentasByYear(year))
                {
                    var sql = @"SELECT ID_VENTA as Id, Fecha, NPuesto as Puesto, Cliente, Vendedor, Descuento, Pago, Tipo, Importe FROM ventas WHERE year(fecha) >= @year AND ID_VENTA >= @value ORDER BY ID_VENTA ASC";

                    ventasAccess = db.Database.SqlQuery<DTO.Venta>(sql,
                        new OleDbParameter("year", year),
                        new OleDbParameter("value", valueInteger))
                        .Take(10)
                        .ToList();
                }
            }
            catch (FarmaciaContextException)
            {
                ventasAccess =  new List<DTO.Venta>();
            }
            
            var ventas = new List<Venta>();
            foreach (var ventaAccess in ventasAccess)
            {
                var venta = new Venta
                {
                    Id = ventaAccess.Id,
                    Tipo = ventaAccess.Tipo.ToString(),
                    FechaHora = ventaAccess.Fecha,
                    Puesto = ventaAccess.Puesto,
                    ClienteId = ventaAccess.Cliente,
                    VendedorId = ventaAccess.Vendedor,
                    TotalDescuento = ventaAccess.Descuento * _factorCentecimal,
                    TotalBruto = ventaAccess.Pago * _factorCentecimal,
                    Importe = ventaAccess.Importe * _factorCentecimal,                        
                };
                    

                if (ventaAccess.Cliente > 0)
                        venta.Cliente =_clientesRepository.GetOneOrDefaultById(ventaAccess.Cliente);
                    
                var ticket = _ticketRepository.GetOneOrdefaultByVentaId(ventaAccess.Id, year);
                if (ticket != null)
                {
                    venta.Ticket = new Ticket
                    {
                        Numero = ticket.Numero,
                        Serie = ticket.Serie
                    };
                }

                venta.VendedorNombre = _vendedoresRepository.GetOneOrDefaultById(ventaAccess.Vendedor)?.Nombre;
                venta.Detalle = GetDetalleDeVentaByVentaId(year, ventaAccess.Id);

                ventas.Add(venta);
            }

            return ventas;            
        }

        public List<Venta> GetAllByIdGreaterOrEqual(long id, DateTime fecha)
        {            
            var rs = new List<DTO.Venta>();
            try
            {
                using (var db = FarmaciaContext.VentasByYear(fecha.Year))
                {
                    var year = fecha.Year;
                    var fechaInicial = fecha.Date.ToString("dd-MM-yyyy HH:mm:ss");

                    var sql = $@"SELECT ID_VENTA as Id, Fecha, NPuesto as Puesto, Cliente, Vendedor, Descuento, Pago, Tipo, Importe FROM ventas WHERE id_venta >= @id AND year(fecha) = @year AND fecha >= #{fechaInicial}# ORDER BY id_venta ASC";

                    rs = db.Database.SqlQuery<DTO.Venta>(sql,
                        new OleDbParameter("id", (int)id),
                        new OleDbParameter("year", year))
                            .Take(10)
                            .ToList();
                }
            }
            catch (FarmaciaContextException)
            {
                return new List<Venta>();
            }
            

            var ventas = new List<Venta>();
            foreach (var ventaRegistrada in rs)
            {
                var venta = new Venta
                {
                    Id = ventaRegistrada.Id,
                    Tipo = ventaRegistrada.Tipo.ToString(),
                    FechaHora = ventaRegistrada.Fecha,
                    Puesto = ventaRegistrada.Puesto,
                    ClienteId = ventaRegistrada.Cliente,
                    VendedorId = ventaRegistrada.Vendedor,
                    TotalDescuento = ventaRegistrada.Descuento * _factorCentecimal,
                    TotalBruto = ventaRegistrada.Pago * _factorCentecimal,
                    Importe = ventaRegistrada.Importe * _factorCentecimal,
                };


                if (ventaRegistrada.Cliente > 0)
                    venta.Cliente = _clientesRepository.GetOneOrDefaultById(ventaRegistrada.Cliente);

                var ticket = _ticketRepository.GetOneOrdefaultByVentaId(ventaRegistrada.Id, fecha.Year);
                if (ticket != null)
                {
                    venta.Ticket = new Ticket
                    {
                        Numero = ticket.Numero,
                        Serie = ticket.Serie
                    };
                }

                venta.VendedorNombre = _vendedoresRepository.GetOneOrDefaultById(ventaRegistrada.Vendedor)?.Nombre;
                venta.Detalle = GetDetalleDeVentaByVentaId(fecha.Year, ventaRegistrada.Id);

                ventas.Add(venta);
            }

            return ventas;
        }

        
        public List<VentaDetalle> GetDetalleDeVentaByVentaId(int year, long venta)
        {
            var ventaInteger = (int)venta;

            try
            {
                using (var db = FarmaciaContext.VentasByYear(year))
                {
                    var sql = @"SELECT ID_Farmaco as Farmaco, Organismo, Cantidad, PVP, DescLin as Descuento, Importe FROM lineas_venta WHERE ID_venta= @venta";
                    var lineas = db.Database.SqlQuery<DTO.LineaVenta>(sql,
                        new OleDbParameter("venta", ventaInteger))
                        .ToList();

                    var linea = 0;
                    var detalle = new List<VentaDetalle>();
                    foreach (var item in lineas)
                    {
                        var ventaDetalle = new VentaDetalle
                        {
                            Linea = ++linea,
                            Importe = item.Importe * _factorCentecimal,
                            PVP = item.PVP * _factorCentecimal,
                            Descuento = item.Descuento * _factorCentecimal,
                            Receta = item.Organismo,
                            Cantidad = item.Cantidad
                        };

                        var farmaco = _farmacoRepository.GetOneOrDefaultById(item.Farmaco);
                        if (farmaco != null)
                        {
                            var pcoste = farmaco.PrecioUnicoEntrada.HasValue && farmaco.PrecioUnicoEntrada != 0
                                ? (decimal)farmaco.PrecioUnicoEntrada.Value * _factorCentecimal
                                : ((decimal?)farmaco.PrecioMedio ?? 0m) * _factorCentecimal;

                            var codigoBarra = _barraRepository.GetOneByFarmacoId(farmaco.Id);
                            var proveedor = _proveedorRepository.GetOneOrDefaultByCodigoNacional(farmaco.Id);

                            var categoria = farmaco.CategoriaId.HasValue
                                ? _categoriaRepository.GetOneOrDefaultById(farmaco.CategoriaId.Value)
                                : null;

                            var subcategoria = farmaco.CategoriaId.HasValue && farmaco.SubcategoriaId.HasValue
                                ? _categoriaRepository.GetSubcategoriaOneOrDefaultByKey(
                                    farmaco.CategoriaId.Value,
                                    farmaco.SubcategoriaId.Value)
                                : null;

                            var familia = _familiaRepository.GetOneOrDefaultById(farmaco.Familia);
                            var laboratorio = _laboratorioRepository.GetOneOrDefaultByCodigo(farmaco.Laboratorio);

                            ventaDetalle.Farmaco = new Farmaco
                            {
                                Id = farmaco.Id,
                                Codigo = item.Farmaco.ToString(),
                                PrecioCoste = pcoste,
                                CodigoBarras = codigoBarra,
                                Proveedor = proveedor,
                                Categoria = categoria,
                                Subcategoria = subcategoria,
                                Familia = familia,
                                Laboratorio = laboratorio,
                                Denominacion = farmaco.Denominacion
                            };
                        }
                        else ventaDetalle.Farmaco = new Farmaco { Id = item.Farmaco, Codigo = item.Farmaco.ToString() };

                        detalle.Add(ventaDetalle);
                    }

                    return detalle;
                }
            }
            catch (FarmaciaContextException)
            {
                return new List<VentaDetalle>();
            }            
        }

        public Ticket GetOneOrDefaultTicketByVentaId(long id)
        {
            var year = int.Parse($"{id}".Substring(0, 4));
            var ventaId = int.Parse($"{id}".Substring(4));

            using (var db = FarmaciaContext.VentasByYear(year))
            {                
                var sql = @"SELECT Id_Ticket as Numero, Serie FROM Tickets_D WHERE Id_Venta = @venta";
                var rs = db.Database.SqlQuery<DTO.Ticket>(sql,
                    new OleDbParameter("venta", ventaId))
                    .FirstOrDefault();

                return rs != null ? new Ticket { Numero = rs.Numero, Serie = rs.Serie } : null;
            }
        }        
    }
}