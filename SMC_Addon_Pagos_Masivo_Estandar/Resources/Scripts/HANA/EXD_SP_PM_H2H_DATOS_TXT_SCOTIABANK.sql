CREATE PROCEDURE EXD_SP_PM_H2H_DATOS_TXT_SCOTIABANK 
(
	NroPM int,
	NroSC int,
	NroCT varchar(50)
)
AS
BEGIN
	declare idEnvio int;
	select count('A') into idEnvio from "@EXD_PM_LOGENVHTH" where U_COD_BANCO = '009' and U_FECHA_ENVIO = TO_DATE(NOW());

	with CTE_DATOS_CAB AS
	(
		select 
			'01'																as "TipoOrden",
			rpad(T0."DocEntry" ||'-'|| T0."LineId",15,' ')						as "Referencia1",
			rpad(left(ifnull(T0."U_EXP_COMENTARIO",''),16),16)					as "Referencia2",
			case T0."U_EXP_MONEDA_PAGO" when 'SOL' 
			then '00' when 'USD' then '01' else '  ' end						as "MonedaPago",
			rpad(replace(T1."Account",'-','')||'01',20,' ')						as "CuentaCargo",
			TO_VARCHAR(now(),'yyyyMMdd')										as "FechaOrdenPago",
			rpad(left(T0."U_EXP_NRODOCUMENTOSN",11),11,' ')						as "RucProveedor",
			left(rpad(left(T0."U_EXP_CARDNAME",60),60,' '),60)					as "NombreProveedor",
			case when T0."U_EXP_CODBANCOPROV" = '009' then '2' else '4' end		as "FormaPago",
			rpad(replace(T0."U_EXP_NROCTAPROV",'-','')||'01',20,' ')			as "CuentaAbono",
			TO_VARCHAR(now(),'yyyyMMdd')										as "FechaFact",
			TO_VARCHAR(now(),'yyyyMMdd')										as "FechaVencFact",
			rpad(ifnull(T0."U_EXP_NROSUNAT",''),20,' ')									as "NumeroFact",
			lpad(floor(T0."U_EXP_IMPORTE"),9,'0')||
			right(REPLACE(TO_VARCHAR(mod(round(T0."U_EXP_IMPORTE",2),1)),'.',''),2)								as "ImporteNeto",
			'77'																as "ModuloRaiz",
			EXD_FN_H2H_TXT_SCOTIABANK_DIGITO_CONTROL(TO_VARCHAR(now(),'yyMMdd')
			,floor(T0."U_EXP_IMPORTE")
			,case T0."U_EXP_MONEDA_PAGO" when 'SOL' then '00' when 'USD' then '01' end
			,case when T0."U_EXP_CODBANCOPROV" <> '009' then '4' else case T3."UsrNumber2" when 'A' then '3' when 'C' then '2' else '1' end end
			,case when T0."U_EXP_CODBANCOPROV" <> '009' then substring(lpad(trim(replace(T0."U_EXP_NROCTAPROV",'-','')),20,'0'),14,20) 
			else left(replace(T0."U_EXP_NROCTAPROV",'-',''),7) end
			,left(replace(T1."Account",'-',''),7))																	as "DigitoControl",
			' '																	as "SubTipoPago",
			'+'																	as "Signo",
			rpad(ifnull(T2."E_Mail",''),50,' ')									as "EmailProveedor",
			T0."U_EXP_IMPORTE"													as "Importe",
			'01'																as "TipoCuenta"
		from "@EXP_PMP1" 	T0
		inner join DSC1		T1 on T1."GLAccount" 		= T0."U_EXP_CODCTABANCO" and T0."U_EXP_COD_SUCURSAL" = T1."Branch"
		inner join OCRD		T2 on T0."U_EXP_CARDCODE"	= T2."CardCode" 
		inner join OCRB		T3 on T2."CardCode"			= T3."CardCode" and T0."U_EXP_CODBANCOPROV" = T3."BankCode"
		where T0."DocEntry" = :NroPM and T0."U_EXP_COD_SUCURSAL" = :NroSC and T0."U_EXP_CODCTABANCO" = :NroCT
		and ifnull(T3."U_EXC_ACTIVO",'') = 'Y'  and T3."UsrNumber1" = "U_EXP_MONEDA"
	),
	CTE_REGISTRO_CONTROL AS
	(
		select
			'99'																		as "Indicador",
			lpad(sum(1),6,'0')															as "CantRegistros",
			lpad(floor(sum("Importe")),13,'0')||right(REPLACE(TO_VARCHAR(mod(round(sum("Importe"),2),1)),'.',''),2)	as "ImporteTotal",
			max("FechaOrdenPago")														as "FechaOrdenPago",
			lpad(SUM(TO_INT("DigitoControl")),6,'0')									as "SumDigitoControl"
		from CTE_DATOS_CAB
	)

	select "Data",'P'||lpad(idEnvio+1,8,'0') as "Nombre"  from
	(
		select 
			"TipoOrden"			||
			"Referencia1"		||
			"Referencia2"		||
			"MonedaPago"		||
			"CuentaCargo"		||
			"FechaOrdenPago"	||
			"RucProveedor"		||
			"NombreProveedor"	||
			"FormaPago"			||
			"CuentaAbono"		||
			"FechaFact" 		||
			"FechaVencFact" 	||
			"NumeroFact"		||
			"ImporteNeto"		||
			"ModuloRaiz"		||
			lpad("DigitoControl",2,'0') ||
			"SubTipoPago"		||
			"Signo"				||
			"EmailProveedor" 	||'Z' 	as "Data",
			1							as "Orden"
		from CTE_DATOS_CAB
		
		union all
		
		select 
			"Indicador"			||
			"CantRegistros"		||
			"ImporteTotal"		||
			"FechaOrdenPago"	||
			"SumDigitoControl"	||'Z' 	as "Data",
			2							as "Orden"
		from CTE_REGISTRO_CONTROL 
		order by 2
	);
END;