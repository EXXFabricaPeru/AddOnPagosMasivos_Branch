CREATE PROCEDURE EXD_SP_PM_H2H_DATOS_TXT_BCP
(
	NroPM int,
	NroSC int
)
AS
BEGIN
	with CTE_CAB AS
	(
		select
			'1'															as "TipoRegistro",
			sum(1)														as "CntDeAbonos",
			TO_VARCHAR(NOW(),'yyyyMMdd')								as "FechaProceso",
			'C'															as "TipoCtaCargo",
			case when T1."U_EXP_MONEDA" = 'SOL' then '0001' else '1001' end	as "Moneda",
			rpad(replace(T3."Account",'-',''),20,' ')					as "NroCtaCargo",
			SUM(0.01)/*sum(T1."U_IMP_PAGO")*/							as "TotalPlanilla",
			ifnull(T1."U_EXP_COMENTARIO",'')							as "Referencia",
			'S'															as "FlagExoITF",
			''															as "TotalControl",
			''															as "Filler",
			T2."DocNum"													as "NroPlanilla",
			''															as "Estado"
		from  
		"@EXP_PMP1" 				T1
		inner join "@EXP_OPMP" 		T2 on T1."DocEntry" = T2."DocEntry"
		inner join DSC1 			T3 on T3."UsrNumber1" = T1."U_EXP_MONEDA"
		where T3."BankCode" = '002' /*and ifnull(T3."UsrNumber2",'') = 'C'*/ and T2."DocEntry" = :NroPM
		and T1."U_EXP_COD_SUCURSAL" = :NroSC
		group by T2."CreateDate",T1."U_EXP_MONEDA",T3."Account",T1."U_EXP_COMENTARIO",T2."DocNum"
	),

	CTE_PROV AS
	(
		select 
			'2'																		as "TipoRegistro",
			case when T3."BankCode" != '002' then 'C' else 'B' end					as "TipoCuentaAbono",
			ifnull(replace(T3."DflAccount",'-',''),'')								as "NroCtaAbono",
			'1'																		as "ModalidadDePago",
			ifnull(T3."U_EXX_TIPODOCU",'') 											as "TipoDocumentoProv",
			T3."LicTradNum"															as "NroDocProv",
			'   '																	as "CorrDocProv",
			T3."CardName"															as "NombreProveedor",
			ifnull(T1."U_EXP_COMENTARIO",'')										as "ReferenciaProveedor",
			''																		as "ReferenciaEmpresa",
			case when T1."U_EXP_MONEDA" = 'SOL' then '0001' else '1001' end			as "Moneda",
			0.01/*T0."DocTotal"*/													as "Importe",
			'S'																		as "Validar",
			''																		as "Filler"
		--from OVPM T0 
		from "@EXP_PMP1"		T1 
		inner join "@EXP_OPMP"	T2 on T1."DocEntry" = T2."DocEntry"
		inner join OCRD			T3 on T1."U_EXP_CARDCODE" = T3."CardCode"
		where T2."DocEntry" = :NroPM and T1."U_EXP_COD_SUCURSAL" = :NroSC
	),
	CTE_BENEF as
	(
		select 
			'3'						as "TipoRegistro",
			case when T1."U_EXP_TIPODOC" = '18' then 'F' else 'D' end						as "TipoDocumento",
			(select TX0."FolioPref"||TX0."FolioNum" from OPCH TX0 where TX0."DocEntry" = T1."U_EXP_DOCENTRYDOC" and TX0."ObjType" = T1."U_EXP_TIPODOC")	as "NroDocAPagar",
			0.01/*T0."DocTotal"*/			as "Importe"
		from 
		"@EXP_PMP1"					T1 
		inner join "@EXP_OPMP"		T2 on T1."DocEntry" = T2."DocEntry"
		--inner join VPM2				T3 on T0."DocEntry"	= T3."DocNum" 
		where T2."DocEntry" = :NroPM
	)	
	select 
		"TipoRegistro"			||
		lpad("CntDeAbonos",6,'0')		||
		"FechaProceso"			||
		"TipoCtaCargo"			||
		"Moneda"				||
		rpad("NroCtaCargo",20,' ')			||
		lpad(round("TotalPlanilla",2),17,'0')			||
		rpad(left("Referencia",40),40,' ')			||
		"FlagExoITF"			||
		lpad(to_bigint(right(trim("NroCtaCargo"),10))+(select sum(to_bigint(right(trim("NroCtaAbono"),10))) from CTE_PROV),15,'0')||
		rpad("Filler",100,' ') ||
		lpad("NroPlanilla",6,'0') ||
		lpad((select count('A') from CTE_PROV),6,' ') ||
		lpad('0',6,' ') ||
		lpad("Estado",80,' ')
		||'Z' as "Data"
	from CTE_CAB
	union all 
	select 
		"TipoRegistro"			||
		"TipoCuentaAbono"		||
		lpad("NroCtaAbono",20,' ') ||
		"ModalidadDePago"		||
		"TipoDocumentoProv"		||
		rpad("NroDocProv",12,' ')			||
		"CorrDocProv"			||
		rpad(left("NombreProveedor",75),75,' ')		||
		rpad(left("ReferenciaProveedor",40),40,' ')	||
		rpad(left("ReferenciaEmpresa",20),20,' ')	||
		"Moneda"				||
		lpad(round("Importe",2),17,'0')				||
		"Validar"||
		lpad("Filler",100,' ')
		||'Z' as "Data"
	from CTE_PROV;
	/*
	union all 
	select 
		"TipoRegistro"		||
		"TipoDocumento"		||
		lpad(ifnull("NroDocAPagar",''),15,'0')	||
		lpad("Importe",17,'0')
		||'Z' as "Data"
	from CTE_BENEF;
	*/
END;