using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Transbank.PatPass;
using Transbank.Webpay.Wsdl.Normal;

namespace PatPassNetExample
{

    public partial class Tbkpatpass : System.Web.UI.Page
    {
        /** Mensaje de Ejecución */
        private string message;

        /** Crea Dictionary con datos de entrada */
        private Dictionary<string, string> request = new Dictionary<string, string>();

        protected void Page_Load()
        {
            HttpContext.Current.Response.Write("<div style='font-family: Tahoma,Helvetica,Arial,Verdana,sans-serif;'><p style='font-weight: bold; font-size: 200%;'>Ejemplo PatPass - Transacci&oacute;n Normal</p>");

            /** Creacion Objeto PatPass */
            PatPass patpass = new PatPass(Configuration.ForTestingPatPassByWebpayNormal());

            /** Información de Host para crear URL */
            string httpHost = HttpContext.Current.Request.ServerVariables["HTTP_HOST"];
            string selfURL = HttpContext.Current.Request.ServerVariables["URL"];

            string action = !string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["action"]) ? HttpContext.Current.Request.QueryString["action"] : "init";

            /** Crea URL de Aplicación */
            string sample_baseurl = "http://" + httpHost + selfURL;

            /** Crea Dictionary con descripción */
            Dictionary<string, string> description = new Dictionary<string, string>
            {
                { "VN", "Venta Normal" }
            };

            /** Crea Dictionary con codigos de resultado */
            Dictionary<string, string> codes = new Dictionary<string, string>
            {
                { "0", "Transacci&oacute;n aprobada" },
                { "-1", "Rechazo de transacci&oacute;n" },
                { "-2", "Transacci&oacute;n debe reintentarse" },
                { "-3", "Error en transacci&oacute;n" },
                { "-4", "Rechazo de transacci&oacute;n" },
                { "-5", "Rechazo por error de tasa" },
                { "-6", "Excede cupo m&aacute;ximo mensual" },
                { "-7", "Excede l&iacute;mite diario por transacci&oacute;n" },
                { "-8", "Rubro no autorizado" },
                { "-100", "-100 Rechazo por inscripci&oacute;n de PatPass by Webpay" }
            };

            string buyOrder;
            string tx_step = "";

            switch (action)
            {
                default:
                    tx_step = "Inicio de Transacci&oacute;n";

                    try
                    {
                        HttpContext.Current.Response.Write("<p style='font-weight: bold; font-size: 150%;'>Etapa: " + tx_step + "</p>");
                        Random random = new Random();

                        var info = new PatPassInfo
                        {
                            /** Identificador de Cliente */
                            ServiceId = "335456675433",
                            /** RUT */
                            CardHolderId = "11.111.111-1",
                            /** Nombres */
                            CardHolderName = "Juan Pedro",
                            /** Apellido Paterno */
                            CardHolderLastName1 = "Alarcón",
                            /** Apeliido Materno */
                            CardHolderLastName2 = "Perez",
                            /** Correo Cliente */
                            CardHolderMail = "example@example.com",
                            /** Telefono Celular Cliente */
                            CellPhoneNumber = "1234567",
                            /** Fecha Expiracon PatPass */
                            ExpirationDate = new System.DateTime(2019, 06, 01)
                        };

                        /** Monto de la transacción */
                        decimal amount = Convert.ToDecimal("9990");

                        /** Orden de compra de la tienda */
                        buyOrder = random.Next(0, 1000).ToString();

                        /** (Opcional) Identificador de sesión, uso interno de comercio */
                        string sessionId = random.Next(0, 1000).ToString();

                        /** URL Final */
                        string urlReturn = sample_baseurl + "?action=result";

                        /** URL Final */
                        string urlFinal = sample_baseurl + "?action=end";

                        request.Add("amount", amount.ToString());
                        request.Add("buyOrder", buyOrder);
                        request.Add("sessionId", sessionId);
                        request.Add("urlReturn", urlReturn);
                        request.Add("urlFinal", urlFinal);
                        request.Add("serviceId", info.ServiceId);
                        request.Add("cardHolderId", info.CardHolderId);
                        request.Add("cardHolderName", info.CardHolderName);
                        request.Add("cardHolderLastName1", info.CardHolderLastName1);
                        request.Add("cardHolderLastName2", info.CardHolderLastName2);
                        request.Add("cardHolderMail", info.CardHolderMail);
                        request.Add("cellPhoneNumber", info.CellPhoneNumber);
                        request.Add("expirationDate", info.ExpirationDate.ToString());

                        /** Ejecutamos metodo initTransaction desde Libreria */
                        wsInitTransactionOutput result = patpass.NormalTransaction.initTransaction(amount, buyOrder, sessionId, urlReturn, urlFinal, info);

                        /** Verificamos respuesta de inicio en PatPass */
                        message = !string.IsNullOrEmpty(result.token) ? "Sesion iniciada con exito en PatPass" : "PatPass no disponible";

                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightyellow;'><strong>request</strong></br></br>" + new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(request) + "</p>");
                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightgrey;'><strong>result</strong></br></br>" + new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(result) + "</p>");

                        HttpContext.Current.Response.Write("" + message + "</br></br>");
                        HttpContext.Current.Response.Write("<form action=" + result.url + " method='post'><input type='hidden' name='token_ws' value=" + result.token + "><input type='submit' value='Ir a PatPass &raquo;'></form>");
                    }
                    catch (Exception ex)
                    {
                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightyellow;'><strong>request</strong></br></br>" + new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(request) + "</p>");
                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightgrey;'><strong>result</strong></br></br> Ocurri&oacute; un error en la transacci&oacute;n (Validar correcta configuraci&oacute;n de parametros). " + ex.Message + "</p>");
                    }
                    break;

                case "result":
                    tx_step = "Obtenci&oacute;n del Resultado de la Transacci&oacute;n";

                    try
                    {
                        HttpContext.Current.Response.Write("<p style='font-weight: bold; font-size: 150%;'>Etapa: " + tx_step + "</p>");

                        /** Obtiene Información POST */
                        string[] keysPost = Request.Form.AllKeys;

                        /** Token de la transacción */
                        string token = Request.Form["token_ws"];
                        request.Add("token", token);

                        transactionResultOutput result = patpass.NormalTransaction.getTransactionResult(token);

                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightyellow;'><strong>request</strong></br></br> " + new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(request) + "</p>");
                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightgrey;'><strong>result</strong></br></br> " + new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(result) + "</p>");

                        if (result.detailOutput[0].responseCode == 0)
                        {
                            message = "Suscripci&oacute;n ACEPTADA por PatPass (se deben guardar datos para mostrar voucher)";

                            HttpContext.Current.Response.Write("<script>window.localStorage.clear()</script>");
                            HttpContext.Current.Response.Write("<script>localStorage.setItem('cardNumber', " + result.cardDetail.cardNumber + ")</script>");
                            HttpContext.Current.Response.Write("<script>localStorage.setItem('authorizedAmount', " + result.detailOutput[0].amount + ")</script>");
                            HttpContext.Current.Response.Write("<script>localStorage.setItem('buyOrder', " + result.detailOutput[0].buyOrder + ")</script>");
                            HttpContext.Current.Response.Write("<script>localStorage.setItem('authorizationCode', " + result.detailOutput[0].authorizationCode + ")</script>");
                            HttpContext.Current.Response.Write("<script>localStorage.setItem('transactionDate', '" + result.transactionDate.ToString("yyyy-MM-ddTHH:mm:ss.fffK") + "')</script>");
                            HttpContext.Current.Response.Write(message + "</br></br>");
                            HttpContext.Current.Response.Write("<form action=" + result.urlRedirection + " method='post'><input type='hidden' name='token_ws' value=" + token + "><input type='submit' value='Continuar &raquo;'></form>");
                        }
                        else
                        {
                            message = "Suscripci&oacute;n RECHAZADA por PatPass (" + codes[result.detailOutput[0].responseCode.ToString()] + ")";
                            HttpContext.Current.Response.Write(message + "</br></br>");
                        }
                    }
                    catch (Exception ex)
                    {
                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightyellow;'><strong>request</strong></br></br>" + new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(request) + "</p>");
                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightgrey;'><strong>result</strong></br></br> Ocurri&oacute; un error en la transacci&oacute;n (Validar correcta configuraci&oacute;n de parametros). " + ex.Message + "</p>");
                    }
                    break;

                case "end":
                    tx_step = "Fin de la Transacci&oacute;n";

                    try
                    {
                        HttpContext.Current.Response.Write("<p style='font-weight: bold; font-size: 150%;'>Etapa: " + tx_step + "</p>");
                        request.Add("", "");

                        /* Captura Resultado  */
                        Dictionary<string, string> result = new Dictionary<string, string> { };
                        string voucher = "";

                        if (Request.Form.AllKeys.Count() == 3)
                        {
                            result.Add("TBK_TOKEN", Request.Form["TBK_TOKEN"]);
                            result.Add("TBK_ID_SESION", Request.Form["TBK_ID_SESION"]);
                            result.Add("TBK_ORDEN_COMPRA", Request.Form["TBK_ORDEN_COMPRA"]);

                            message = "Transacci&oacute;n Anulada por el cliente";

                            HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightyellow;'><strong>request</strong></br></br>" + new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(request) + "</p>");
                            HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightgrey;'><strong>result</strong></br></br>" + new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(result) + " </p>");
                            HttpContext.Current.Response.Write(message + "</br>");
                            HttpContext.Current.Response.Write("<script>window.localStorage.clear()</script>");
                        }
                        else
                        {
                            result.Add("token_ws", Request.Form["token_ws"]);

                            voucher = "<pre>VOUCHER COMERCIO</br> ";
                            voucher += "Monto: $<span id='authorizedAmount'></span></br> ";
                            voucher += "C&oacute;digo Autorizaci&oacute;n: <span id='authorizationCode'></span></br> ";
                            voucher += "Orden de Compra: <span id='buyOrder'></span></br> ";
                            voucher += "Nro Tarjeta: XXXX -<span id='cardNumber'></span></br> ";
                            voucher += "Fecha: <span id='transactionDate'></span></pre>";

                            message = "Transacci&oacute;n Finalizada";

                            HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightyellow;'><strong>request</strong></br></br>" + new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(request) + "</p>");
                            HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightgrey;'><strong>result</strong></br></br>" + new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(result) + " </p>");
                            HttpContext.Current.Response.Write(message + "</br>" + voucher);
                            HttpContext.Current.Response.Write("<script>document.getElementById('authorizationCode').innerHTML = localStorage.getItem('authorizationCode')</script>");
                            HttpContext.Current.Response.Write("<script>document.getElementById('authorizedAmount').innerHTML = localStorage.getItem('authorizedAmount')</script>");
                            HttpContext.Current.Response.Write("<script>document.getElementById('buyOrder').innerHTML = localStorage.getItem('buyOrder')</script>");
                            HttpContext.Current.Response.Write("<script>document.getElementById('cardNumber').innerHTML = localStorage.getItem('cardNumber')</script>");
                            HttpContext.Current.Response.Write("<script>document.getElementById('transactionDate').innerHTML = localStorage.getItem('transactionDate')</script>");
                            HttpContext.Current.Response.Write("<script>window.localStorage.clear()</script>");
                        }
                    }
                    catch (Exception ex)
                    {
                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightyellow;'><strong>request</strong></br></br>" + new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(request) + "</p>");
                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightgrey;'><strong>result</strong></br></br> Ocurri&oacute; un error en la transacci&oacute;n (Validar correcta configuraci&oacute;n de parametros). " + ex.Message + "</p>");
                    }
                    break;
            }
            HttpContext.Current.Response.Write("</br><a href='/'>&laquo; Volver al &Iacute;ndice</a></div>");
        }
    }
}
