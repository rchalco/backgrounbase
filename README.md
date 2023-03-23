# ibnorca_services
proyecto ibnorca, capa de servicios -> .net 5 + MySQL

Changelog 

Rev 2021.07.29

* Changed
	Taks Id: #614pg7 
	
	* Cambio de variable IdCiclo por IdSistema
	Se realizaron cambios acorde a lo solicitado por Ibnorca, para que sus aplicativos envien el IdSistema en la consulta API a Sigad. Esta actividad se atendio para los procesos TCS - Evaluacion y Recomendacion, TCS - Desicion Favorable, TCS - Decision No Favorable, TCS - Nota Suspension, TCS - Nota Retiro de la siguiente manera:
	Anteriormente en la invocacion de consulta API en el metodo [SERVER:PORT]/api/Plantilla/GenerarDocumento para los siguientes procesos citados requeria un campo IdCiclo que reflejaba el IdCiclo interno de Sigad. 
		"IdCiclo": 456 donde 456 era el IdCiclo
	Ahora en la misma variable, Ibnorca debe enviar el valor IdSistema
		"IdCiclo": 789 donde 789 es el IdSistema, dato que ya cuenta Ibnorca en sus sistemas.
	
	Note: Se identifico con los casos de prueba que existen ejemplos que tienen problema de data incompleta. Esto obedece al flujo de informacion que se tiene. Por esto se recomienda probar con casos completos o consistentes.
	
	* Path Templates
	Se cambia el valor de la variable PLANTILLAS__PATH
		De >>> D:\\Ibnorka_Plantillas_Oficial\\ 
		A  >>> /Ibnorka_Plantillas_Oficial/
	Esto en sujecion a las buenas practicas de programacion, compatibilidad de Sistemas Operativos y Prevision de Puesta a Produccion.
	
	Note: En caso que el desarrollador tenga hospedado las plantillas en otra direccion, debe cambiar la misma acorde a su ambiente.
