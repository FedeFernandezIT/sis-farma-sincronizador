﻿using System;
using Sisfarma.Sincronizador.Domain.Core.Services;
using Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices.Sisfarma;

using INF = Sisfarma.Sincronizador.Infrastructure.Fisiotes;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Factories
{
    public static class SisfarmaFactory
    {
        private static string _host;
        private static string _token;

        public static SisfarmaService Create()
        {
            var configuraciones = new INF.ConfiguracionesExternalService(new RestClient.RestSharp.RestClient(), INF.FisiotesConfig.TestConfig(_host, _token));
            var clientes = new ClientesExternalService(new RestClient.RestSharp.RestClient(), INF.FisiotesConfig.TestConfig(_host, _token));
            var huecos = new INF.HuecosExternalService(new RestClient.RestSharp.RestClient(), INF.FisiotesConfig.TestConfig(_host, _token));
            var puntosPendientes = new PuntosPendientesExternalService(new RestClient.RestSharp.RestClient(), INF.FisiotesConfig.TestConfig(_host, _token));
            var programacion = new INF.ProgramacionExternalService(new RestClient.RestSharp.RestClient(), INF.FisiotesConfig.TestConfig(_host, _token));
            var categorias = new INF.CategoriasExternalService(new RestClient.RestSharp.RestClient(), INF.FisiotesConfig.TestConfig(_host, _token));
            var medicamentos = new MedicamentosExternalServices(new RestClient.RestSharp.RestClient(), INF.FisiotesConfig.TestConfig(_host, _token));
            var faltas = new FaltasExternalService(new RestClient.RestSharp.RestClient(), INF.FisiotesConfig.TestConfig(_host, _token));

            return new SisfarmaService(
                clientes: clientes,
                huecos: huecos,
                puntosPendientes: puntosPendientes,
                configuraciones: configuraciones,
                medicamentos: medicamentos,
                categorias: categorias,
                faltas: faltas,
                programacion: programacion);
        }

        public static void Setup(string host, string token)
        {
            if (string.IsNullOrWhiteSpace(host))            
                throw new ArgumentException("message", nameof(host));

            if (string.IsNullOrWhiteSpace(token))            
                throw new ArgumentException("message", nameof(token));            

            _host = host;
            _token = token;
        }
    }
}
