CREATE PROCEDURE SBO_EXX_PM_BCP_DEV
(
	NroPM int,
	NroSC int,
	NroCT varchar(50),
	MedPag varchar(4)
)
AS
BEGIN
	declare mndLoc varchar(5);
	declare versionExtendida varchar(1);
	declare nuevoTelecredito varchar(1);
	declare agruparPagosBCP varchar(1);

	select U_VALOR into versionExtendida from "@SMC_APM_CONFIAPM" where "Code" = '18';
	select U_VALOR into nuevoTelecredito from "@SMC_APM_CONFIAPM" where "Code" = '20';
	select U_VALOR into agruparPagosBCP from "@SMC_APM_CONFIAPM" where "Code" = '21';
	select "MainCurncy" into mndLoc from OADM;
	
	IF :nuevoTelecredito = 'Y'
	THEN
		with CTE_CAB AS
		(
			select
				top 1
				'#'															as "PlanillaNueva",
				'1'															as "TipoDeRegistro",
				'P'															as "TipoDePagoMasivo",
				'C'															as "TipoCtaCargo",
				--rpad(replace(T3."Account",'-',''),20,' ')					as "NroCtaCargo",
				LEFT(rpad(replace(T3."Account",'-',''),20,' '),3)||'0'||SUBSTRING(rpad(replace(T3."Account",'-',''),20,' '),4,LENGTH(rpad(replace(T3."Account",'-',''),20,' '))) as "NroCtaCargo",
				case when T1."U_EXP_MONEDA" = :mndLoc then 'S/' else 'US' end	as "Moneda",
				sum(TO_DECIMAL(T1."U_EXP_IMPORTE",14,2))					as "TotalPlanilla",
				TO_VARCHAR(NOW(),'ddMMyyyy')								as "FechaProceso",				
				ifnull(T1."U_EXP_COMENTARIO",'')							as "Referencia",
				sum(1)														as "CntDeAbonos",
				'N'															as "FlagExoITF",
				''															as "TotalControl",
				''															as "Filler",
				T2."DocNum"													as "NroPlanilla",
				'1'															as "NotaDeCargo",
				''															as "Estado"
			from  
			"@EXP_PMP1" 				T1
			inner join "@EXP_OPMP" 		T2 on T1."DocEntry" = T2."DocEntry"
			inner join DSC1 			T3 on T3."BankCode" = T1."U_EXP_CODBANCO" and ifnull(T1."U_EXP_COD_SUCURSAL",'0') = ifnull(T3."Branch",'0') and T1."U_EXP_CODCTABANCO" =  T3."GLAccount"
			where T3."BankCode" = '002' --and ifnull(T3."UsrNumber2",'') = 'C'
			and T2."DocEntry" = :NroPM
			and T1."U_EXP_COD_SUCURSAL" = :NroSC
			and T1."U_EXP_CODCTABANCO" = :NroCT
			and T1."U_EXP_MEDIODEPAGO" = :MedPag
			and coalesce(T1."U_EXP_SLC_PAGO",'') = 'Y'
			group by T2."DocEntry",T1."U_EXP_COD_SUCURSAL",T2."CreateDate",T1."U_EXP_MONEDA",T3."Account",T1."U_EXP_COMENTARIO",T2."DocNum"
		),
		
		CTE_PROV AS
		(
			select 
				'2'																						as "TipoRegistro",
				case T1."U_EXP_CODBANCOPROV" when '002' then ifnull(T4."UsrNumber2",' ') else 'B' end	as "TipoCuentaAbono",
				ifnull(replace(case when T4."BankCode" = '002' 
				then --T4."Account"
					case (case T1."U_EXP_CODBANCOPROV" when '002' then ifnull(T4."UsrNumber2",' ') else 'B' end) 
					when 'A' then rpad(replace(T4."Account",'-',''),20,' ')
					when 'C' then LEFT(rpad(replace(T4."Account",'-',''),20,' '),3)||'0'||SUBSTRING(rpad(replace(T4."Account",'-',''),20,' '),4,LENGTH(rpad(replace(T4."Account",'-',''),20,' ')))
					else T4."U_EXM_INTERBANCARIA" end
				else T4."U_EXM_INTERBANCARIA" end ,'-',''),'')							as "NroCtaAbono",
				LIMPIA_CADENA(T3."CardName")																		as "NombreProveedor",
				case when T1."U_EXP_MONEDA" = :mndLoc then 'S/' else 'US' end							as "Moneda",
				TO_DECIMAL(T1."U_EXP_IMPORTE",14,2)														as "Importe",
				ifnull(case T3."U_EXX_TIPODOCU" 
				when '6' then 'RUC'
				when '1' then 'DNI'
				when '4' then 'CE '
				when '7' then 'PAS'
				else 'FIC' END,'') 																		as "TipoDocumentoProv",
				T3."LicTradNum"																			as "NroDocProv",
				'F'																						as "TipoDocumentoPagar",
				right(ifnull(replace(T1."U_EXP_NROSUNAT",'-',''),''),10)								as "NroDeDocumentoPagar",
				'1'																						as "TipoDeAbono",
				ifnull(T1."U_EXP_NROSUNAT",'')															as "ReferenciaEmpresa",
				'0'																						as "FlagAbono",
				'0'																						as "FlagDelivery",
				'1'																						as "FlagValidarIDC",
				''																						as "Direccion",
				''																						as "Distrito",
				''																						as "Provincia",
				''																						as "Departamento",
				''																						as "Contacto",
				T1."LineId"																				as "NroLinea"
			from "@EXP_PMP1"		T1 
			inner join "@EXP_OPMP"	T2 on T1."DocEntry" = T2."DocEntry"
			inner join OCRD			T3 on T1."U_EXP_CARDCODE" = T3."CardCode"
			inner join OCRB			T4 on T4."CardCode" = T3."CardCode" and T1."U_EXP_MONEDA" = T4."UsrNumber1"
			where T2."DocEntry" = :NroPM 
			and T1."U_EXP_COD_SUCURSAL" = :NroSC
			and T1."U_EXP_CODCTABANCO" = :NroCT
			and T1."U_EXP_MEDIODEPAGO" = :MedPag
			and coalesce(T1."U_EXP_SLC_PAGO",'') = 'Y'
			and T4."U_EXC_ACTIVO" = 'Y'
		)
		
		select "Data","NroLinea","Orden" from
		(
			select
				0 as "Orden",0 as "NroLinea",
				"PlanillaNueva"			||
				"TipoDeRegistro"		||
				"TipoDePagoMasivo"		||
				"TipoCtaCargo"			||
				rpad("NroCtaCargo",20,' ')			||
				"Moneda"				||
				lpad(replace(trim(round("TotalPlanilla",2)),'.',''),15,'0')			||
				"FechaProceso"	||
				rpad(left("Referencia",40),20,' ')			||
				lpad(to_bigint(right(trim("NroCtaCargo"),10))+(select sum(to_bigint(right(trim(TX0."NroCtaAbono")
				,case when "TipoCuentaAbono" = 'A' then 11 else 10 end))) from CTE_PROV TX0 where ifnull(TX0."NroCtaAbono",'') <>''),15,'0')||
				lpad("CntDeAbonos",6,'0') ||
				'0' ||
				rpad("Filler",15,' ') ||
				"NotaDeCargo" ||
				--lpad("NroPlanilla",6,'0') ||
				--lpad((select count('A') from CTE_PROV),6,' ') ||
				--lpad('0',6,' ') ||
				--lpad("Estado",80,' ')
				'Z' as "Data"
			from CTE_CAB

			union all 
			
			select 1 as "Orden","NroLinea",
				' '						||
				"TipoRegistro"			||
				"TipoCuentaAbono"		||
				rpad("NroCtaAbono",20,' ') ||
				rpad(left("NombreProveedor",40),40,' ')		||
				"Moneda"				||
				lpad(replace(trim(round("Importe",2)),'.',''),15,'0')				||
				"TipoDocumentoProv"		||
				rpad("NroDocProv",12,' ')					||
				"TipoDocumentoPagar"						||
				rpad("NroDeDocumentoPagar",10,'0')			||
				"TipoDeAbono"					||
				rpad(left("ReferenciaEmpresa",20),40,' ')	||
				"FlagAbono"					||
				"FlagDelivery"				||
				"FlagValidarIDC"			||
				rpad("Direccion",40,' ')					||
				rpad("Distrito",20,' ')						||
				rpad("Provincia",20,' ')					||
				rpad("Departamento",20,' ')					||
				rpad("Contacto",40,' ')
				||'Z' as "Data"
			from CTE_PROV where ifnull("NroCtaAbono",'') <>''
		) order by 2,3;
	ELSE
		IF :versionExtendida = 'Y'
		THEN
			IF :agruparPagosBCP = 'Y'
			THEN
				with CTE_CAB AS
				(
					select
						top 1
						'1'															as "TipoRegistro",
						sum(1)														as "CntDeAbonos",
						TO_VARCHAR(NOW(),'yyyyMMdd')								as "FechaProceso",
						'C'															as "TipoCtaCargo",
						case when T1."U_EXP_MONEDA" = :mndLoc then '0001' else '1001' end	as "Moneda",
						rpad(replace(T3."Account",'-',''),20,' ')					as "NroCtaCargo",
						sum(TO_DECIMAL(T1."U_EXP_IMPORTE",14,4))					as "TotalPlanilla",
						ifnull(T1."U_EXP_COMENTARIO",'')							as "Referencia",
						'N'															as "FlagExoITF",
						''															as "TotalControl",
						''															as "Filler",
						T2."DocNum"													as "NroPlanilla",
						''															as "Estado"
					from  
					"@EXP_PMP1" 				T1
					inner join "@EXP_OPMP" 		T2 on T1."DocEntry" = T2."DocEntry"
					inner join DSC1 			T3 on T3."BankCode" = T1."U_EXP_CODBANCO" and ifnull(T1."U_EXP_COD_SUCURSAL",'0') = ifnull(T3."Branch",'0') and T1."U_EXP_CODCTABANCO" =  T3."GLAccount"
					where T3."BankCode" = '002' --and ifnull(T3."UsrNumber2",'') = 'C'
					and T2."DocEntry" = :NroPM
					and T1."U_EXP_COD_SUCURSAL" = :NroSC
					and T1."U_EXP_CODCTABANCO" = :NroCT
					and T1."U_EXP_MEDIODEPAGO" = :MedPag
					and coalesce(T1."U_EXP_SLC_PAGO",'') = 'Y'
					group by T2."DocEntry",T1."U_EXP_COD_SUCURSAL",T2."CreateDate",T1."U_EXP_MONEDA",T3."Account",T1."U_EXP_COMENTARIO",T2."DocNum"
				),
		
				CTE_PROV AS
				(
					select 
						'2'																			as "TipoRegistro",
						case when :MedPag = 'CG' then ' '
						else
							case T1."U_EXP_CODBANCOPROV" 
								when '002' then ifnull(T4."UsrNumber2",' ') 
								else 'B' 
							end	
						end																			as "TipoCuentaAbono",
						case when :MedPag = 'CG' then '0'
						else
							ifnull(replace(case when T4."BankCode" = '002' 
							then T4."Account" else T4."U_EXM_INTERBANCARIA"end ,'-',''),'')
						end																			as "NroCtaAbono",
						case when :MedPag = 'CG' then '2' else '1' end								as "ModalidadDePago",
						ifnull(case T3."U_EXX_TIPODOCU" 
						when '4' then '3'
						when '7' then '4'
						else T3."U_EXX_TIPODOCU" END,'') 											as "TipoDocumentoProv",
						T3."LicTradNum"																as "NroDocProv",
						'   '																		as "CorrDocProv",
						LIMPIA_CADENA(T3."CardName")												as "NombreProveedor",
						/*ifnull(T1."U_EXP_NROSUNAT",'')*/''										as "ReferenciaProveedor",
						/*ifnull(T1."U_EXP_NROSUNAT",'')*/''										as "ReferenciaEmpresa",
						case when T1."U_EXP_MONEDA" = :mndLoc then '0001' else '1001' end			as "Moneda",
						SUM(TO_DECIMAL(T1."U_EXP_IMPORTE",14,4))									as "Importe",
						'S'																			as "Validar",
						''																			as "Filler",
						T3."CardCode"																as "CodigoProveedor"
					--from OVPM T0 
					from "@EXP_PMP1"		T1 
					inner join "@EXP_OPMP"	T2 on T1."DocEntry" = T2."DocEntry"
					inner join OCRD			T3 on T1."U_EXP_CARDCODE" = T3."CardCode"
					inner join OCRB			T4 on T4."CardCode" = T3."CardCode" and T1."U_EXP_MONEDA" = T4."UsrNumber1"
					where T2."DocEntry" = :NroPM 
					and T1."U_EXP_COD_SUCURSAL" = :NroSC
					and T1."U_EXP_CODCTABANCO" = :NroCT
					and T1."U_EXP_MEDIODEPAGO" = :MedPag
					and coalesce(T1."U_EXP_SLC_PAGO",'') = 'Y'
					and T4."U_EXC_ACTIVO" = 'Y'
					group by T3."CardCode",T1."U_EXP_CODBANCOPROV",T4."UsrNumber2",T4."BankCode",T4."Account"
					,T4."U_EXM_INTERBANCARIA",T3."U_EXX_TIPODOCU",T3."LicTradNum",T3."CardName",T1."U_EXP_MONEDA"
				)
				,
				CTE_BENEF as
				(
					select 
						'3'																as "TipoRegistro",									
						case when T1."U_EXP_TIPODOC" = '18' then 'F' else 'D' end		as "TipoDocumento",
						case when :MedPag = 'CG' 
						then 
							replace(T1."U_EXP_NROSUNAT",'-','0')
						else 
							T1."U_EXP_NROSUNAT" 
						end																as "NroDocAPagar",
						TO_DECIMAL(T1."U_EXP_IMPORTE",14,4)								as "Importe",
						T1."U_EXP_CARDCODE"												as "CodigoProveedor"						
					from 
					"@EXP_PMP1"	T1 where T1."DocEntry" = :NroPM
					and T1."U_EXP_COD_SUCURSAL" = :NroSC
					and T1."U_EXP_CODCTABANCO" = :NroCT
					and T1."U_EXP_MEDIODEPAGO" = :MedPag
					and coalesce(T1."U_EXP_SLC_PAGO",'') = 'Y'
				)
		
				select "Data","Orden","CodigoProveedor","Orden2" from
				(
					select
						0 as "Orden",'A' as "CodigoProveedor",0 as "Orden2",
						"TipoRegistro"			||
						lpad("CntDeAbonos",6,'0')		||
						"FechaProceso"			||
						"TipoCtaCargo"			||
						"Moneda"				||
						rpad("NroCtaCargo",20,' ')			||
						lpad(TO_DECIMAL(round("TotalPlanilla",2),14,2),17,'0')			||
						rpad(left("Referencia",40),40,' ')			||
						"FlagExoITF"			||
						lpad(to_bigint(right(trim("NroCtaCargo"),10))+(select sum(to_bigint(right(trim(TX0."NroCtaAbono")
						,case when "TipoCuentaAbono" = 'A' then 11 else 10 end))) from CTE_PROV TX0 where ifnull(TX0."NroCtaAbono",'') <>''),15,'0')||
						--rpad("Filler",100,' ') ||
						--lpad("NroPlanilla",6,'0') ||
						--lpad((select count('A') from CTE_PROV),6,' ') ||
						--lpad('0',6,' ') ||
						--lpad("Estado",80,' ')
						'Z' as "Data"
					from CTE_CAB
				
					union all 
				
					select 1 as "Orden","CodigoProveedor",1 as "Orden2",
						"TipoRegistro"			||
						"TipoCuentaAbono"		||
						rpad(case when :MedPag = 'CG' then '' else "NroCtaAbono" end,20,' ') ||
						"ModalidadDePago"		||
						"TipoDocumentoProv"		||
						rpad("NroDocProv",12,' ')			||
						"CorrDocProv"			||
						rpad(left("NombreProveedor",75),75,' ')		||
						rpad(left("ReferenciaProveedor",40),40,' ')	||
						rpad(left("ReferenciaEmpresa",20),20,' ')	||
						"Moneda"				||
						lpad(TO_DECIMAL(round("Importe",2),14,2),17,'0')				||
						"Validar"
						||'Z' as "Data"
					from CTE_PROV where ifnull("NroCtaAbono",'') <>''
					union all 
					select 1 as "Orden","CodigoProveedor",2 as "Orden2",
						"TipoRegistro"		||
						"TipoDocumento"		||
						rpad(ifnull("NroDocAPagar",''),15,'0')	||
						lpad(TO_DECIMAL("Importe",14,2),17,'0')
						||'Z' as "Data"
					from CTE_BENEF
				) order by 2,3,4;
			ELSE
				with CTE_CAB AS
				(
					select
						top 1
						'1'															as "TipoRegistro",
						sum(1)														as "CntDeAbonos",
						TO_VARCHAR(NOW(),'yyyyMMdd')								as "FechaProceso",
						'C'															as "TipoCtaCargo",
						case when T1."U_EXP_MONEDA" = :mndLoc then '0001' else '1001' end	as "Moneda",
						rpad(replace(T3."Account",'-',''),20,' ')					as "NroCtaCargo",
						sum(TO_DECIMAL(T1."U_EXP_IMPORTE",14,4))					as "TotalPlanilla",
						ifnull(T1."U_EXP_COMENTARIO",'')							as "Referencia",
						'N'															as "FlagExoITF",
						''															as "TotalControl",
						''															as "Filler",
						T2."DocNum"													as "NroPlanilla",
						''															as "Estado"
					from  
					"@EXP_PMP1" 				T1
					inner join "@EXP_OPMP" 		T2 on T1."DocEntry" = T2."DocEntry"
					inner join DSC1 			T3 on T3."BankCode" = T1."U_EXP_CODBANCO" and ifnull(T1."U_EXP_COD_SUCURSAL",'0') = ifnull(T3."Branch",'0') and T1."U_EXP_CODCTABANCO" =  T3."GLAccount"
					where T3."BankCode" = '002' --and ifnull(T3."UsrNumber2",'') = 'C'
					and T2."DocEntry" = :NroPM
					and T1."U_EXP_COD_SUCURSAL" = :NroSC
					and T1."U_EXP_CODCTABANCO" = :NroCT
					and T1."U_EXP_MEDIODEPAGO" = :MedPag
					and coalesce(T1."U_EXP_SLC_PAGO",'') = 'Y'
					group by T2."DocEntry",T1."U_EXP_COD_SUCURSAL",T2."CreateDate",T1."U_EXP_MONEDA",T3."Account",T1."U_EXP_COMENTARIO",T2."DocNum"
				),
		
				CTE_PROV AS
				(
					select
						'2'																			as "TipoRegistro", 
						case when :MedPag = 'CG' then ' '
						else
							case T1."U_EXP_CODBANCOPROV" 
								when '002' then ifnull(T4."UsrNumber2",' ') 
								else 'B' 
							end	
						end																			as "TipoCuentaAbono",	
						case when :MedPag = 'CG' then '0'
						else
							ifnull(replace(case when T4."BankCode" = '002' 
							then T4."Account" else T4."U_EXM_INTERBANCARIA"end ,'-',''),'')
						end																			as "NroCtaAbono",
						case when :MedPag = 'CG' then '2' else '1' end								as "ModalidadDePago",
						ifnull(case T3."U_EXX_TIPODOCU" 
						when '4' then '3'
						when '7' then '4'
						else T3."U_EXX_TIPODOCU" END,'') 											as "TipoDocumentoProv",
						T3."LicTradNum"																as "NroDocProv",
						'   '																		as "CorrDocProv",
						LIMPIA_CADENA(T3."CardName")												as "NombreProveedor",
						ifnull(T1."U_EXP_NROSUNAT",'')												as "ReferenciaProveedor",
						ifnull(T1."U_EXP_NROSUNAT",'')												as "ReferenciaEmpresa",
						case when T1."U_EXP_MONEDA" = :mndLoc then '0001' else '1001' end				as "Moneda",
						TO_DECIMAL(T1."U_EXP_IMPORTE",14,4)											as "Importe",
						'S'																			as "Validar",
						''																			as "Filler",
						T1."LineId"																	as "NroLinea"
					--from OVPM T0 
					from "@EXP_PMP1"		T1 
					inner join "@EXP_OPMP"	T2 on T1."DocEntry" = T2."DocEntry"
					inner join OCRD			T3 on T1."U_EXP_CARDCODE" = T3."CardCode"
					inner join OCRB			T4 on T4."CardCode" = T3."CardCode" and T1."U_EXP_MONEDA" = T4."UsrNumber1"
					where T2."DocEntry" = :NroPM 
					and T1."U_EXP_COD_SUCURSAL" = :NroSC
					and T1."U_EXP_CODCTABANCO" = :NroCT
					and T1."U_EXP_MEDIODEPAGO" = :MedPag
					and coalesce(T1."U_EXP_SLC_PAGO",'') = 'Y'
					and T4."U_EXC_ACTIVO" = 'Y'
				)
				,
				CTE_BENEF as
				(
					select 
						'3'																as "TipoRegistro",									
						case when T1."U_EXP_TIPODOC" = '18' then 'F' else 'D' end		as "TipoDocumento",
						case when :MedPag = 'CG' 
						then 
							replace(T1."U_EXP_NROSUNAT",'-','0')
						else 
							T1."U_EXP_NROSUNAT" 
						end																as "NroDocAPagar",
						TO_DECIMAL(T1."U_EXP_IMPORTE",14,4)								as "Importe",
						"LineId"														as "NroLinea"
					from 
					"@EXP_PMP1"	T1 where T1."DocEntry" = :NroPM
					and T1."U_EXP_COD_SUCURSAL" = :NroSC
					and T1."U_EXP_CODCTABANCO" = :NroCT
					and T1."U_EXP_MEDIODEPAGO" = :MedPag
					and coalesce(T1."U_EXP_SLC_PAGO",'') = 'Y'
				)
		
				select "Data","NroLinea","Orden" from
				(
					select
						0 as "Orden",0 as "NroLinea",
						"TipoRegistro"			||
						lpad("CntDeAbonos",6,'0')		||
						"FechaProceso"			||
						"TipoCtaCargo"			||
						"Moneda"				||
						rpad("NroCtaCargo",20,' ')			||
						lpad(TO_DECIMAL(round("TotalPlanilla",2),14,2),17,'0')			||
						rpad(left("Referencia",40),40,' ')			||
						"FlagExoITF"			||
						lpad(to_bigint(right(trim("NroCtaCargo"),10))+(select sum(to_bigint(right(trim(TX0."NroCtaAbono")
						,case when "TipoCuentaAbono" = 'A' then 11 else 10 end))) from CTE_PROV TX0 where ifnull(TX0."NroCtaAbono",'') <>''),15,'0')||
						--rpad("Filler",100,' ') ||
						--lpad("NroPlanilla",6,'0') ||
						--lpad((select count('A') from CTE_PROV),6,' ') ||
						--lpad('0',6,' ') ||
						--lpad("Estado",80,' ')
						'Z' as "Data"
					from CTE_CAB
				
					union all 
				
					select 1 as "Orden","NroLinea",
						"TipoRegistro"			||
						"TipoCuentaAbono"		||
						rpad(case when :MedPag = 'CG' then '' else "NroCtaAbono" end,20,' ') ||
						"ModalidadDePago"		||
						"TipoDocumentoProv"		||
						rpad("NroDocProv",12,' ')			||
						"CorrDocProv"			||
						rpad(left("NombreProveedor",75),75,' ')		||
						rpad(left("ReferenciaProveedor",40),40,' ')	||
						rpad(left("ReferenciaEmpresa",20),20,' ')	||
						"Moneda"				||
						lpad(TO_DECIMAL(round("Importe",2),14,2),17,'0')				||
						"Validar"
						||'Z' as "Data"
					from CTE_PROV where ifnull("NroCtaAbono",'') <>''
					union all 
					select 2 as "Orden","NroLinea",
						"TipoRegistro"		||
						"TipoDocumento"		||
						rpad(ifnull("NroDocAPagar",''),15,'0')	||
						lpad(TO_DECIMAL(round("Importe",2),14,2),17,'0')
						||'Z' as "Data"
					from CTE_BENEF
				) order by 2,3;
			END IF;
		ELSE
			with CTE_CAB AS
			(
				select
					top 1
					'1'															as "TipoRegistro",
					sum(1)														as "CntDeAbonos",
					TO_VARCHAR(NOW(),'yyyyMMdd')								as "FechaProceso",
					'C'															as "TipoCtaCargo",
					case when T1."U_EXP_MONEDA" = :mndLoc then '0001' else '1001' end	as "Moneda",
					rpad(replace(T3."Account",'-',''),20,' ')					as "NroCtaCargo",
					sum(TO_DECIMAL(T1."U_EXP_IMPORTE",14,2))					as "TotalPlanilla",
					ifnull(T1."U_EXP_COMENTARIO",'')							as "Referencia",
					'N'															as "FlagExoITF",
					''															as "TotalControl",
					''															as "Filler",
					T2."DocNum"													as "NroPlanilla",
					''															as "Estado"
				from  
				"@EXP_PMP1" 				T1
				inner join "@EXP_OPMP" 		T2 on T1."DocEntry" = T2."DocEntry"
				inner join DSC1 			T3 on T3."BankCode" = T1."U_EXP_CODBANCO" and ifnull(T1."U_EXP_COD_SUCURSAL",'0') = ifnull(T3."Branch",'0') and T1."U_EXP_CODCTABANCO" =  T3."GLAccount"
				where T3."BankCode" = '002' --and ifnull(T3."UsrNumber2",'') = 'C'
				and T2."DocEntry" = :NroPM
				and T1."U_EXP_COD_SUCURSAL" = :NroSC
				and T1."U_EXP_CODCTABANCO" = :NroCT
				and T1."U_EXP_MEDIODEPAGO" = :MedPag
				and coalesce(T1."U_EXP_SLC_PAGO",'') = 'Y'
				group by T2."DocEntry",T1."U_EXP_COD_SUCURSAL",T2."CreateDate",T1."U_EXP_MONEDA",T3."Account",T1."U_EXP_COMENTARIO",T2."DocNum"
			),

			CTE_PROV AS
			(
				select 
					'2'																			as "TipoRegistro",
					case when :MedPag = 'CG' then ' '
					else
						case T1."U_EXP_CODBANCOPROV" 
							when '002' then ifnull(T4."UsrNumber2",' ') 
							else 'B' 
						end	
					end																			as "TipoCuentaAbono",					
					case when :MedPag = 'CG' then '0'
					else
						ifnull(replace(case when T4."BankCode" = '002' 
						then T4."Account" else T4."U_EXM_INTERBANCARIA"end ,'-',''),'')
					end																			as "NroCtaAbono",
					case when :MedPag = 'CG' then '2' else '1' end								as "ModalidadDePago",
					ifnull(case T3."U_EXX_TIPODOCU" 
					when '4' then '3'
					when '7' then '4'
					else T3."U_EXX_TIPODOCU" END,'') 												as "TipoDocumentoProv",
					T3."LicTradNum"																as "NroDocProv",
					'   '																		as "CorrDocProv",
					LIMPIA_CADENA(T3."CardName")												as "NombreProveedor",
					ifnull(T1."U_EXP_NROSUNAT",'')												as "ReferenciaProveedor",
					ifnull(T1."U_EXP_NROSUNAT",'')												as "ReferenciaEmpresa",
					case when T1."U_EXP_MONEDA" = :mndLoc then '0001' else '1001' end			as "Moneda",
					TO_DECIMAL(T1."U_EXP_IMPORTE",14,2)											as "Importe",
					'S'																			as "Validar",
					''																			as "Filler"
				--from OVPM T0 
				from "@EXP_PMP1"		T1 
				inner join "@EXP_OPMP"	T2 on T1."DocEntry" = T2."DocEntry"
				inner join OCRD			T3 on T1."U_EXP_CARDCODE" = T3."CardCode"
				inner join OCRB			T4 on T4."CardCode" = T3."CardCode" and T1."U_EXP_MONEDA" = T4."UsrNumber1"
				where T2."DocEntry" = :NroPM 
				and T1."U_EXP_COD_SUCURSAL" = :NroSC
				and T1."U_EXP_CODCTABANCO" = :NroCT
				and T1."U_EXP_MEDIODEPAGO" = :MedPag
				and coalesce(T1."U_EXP_SLC_PAGO",'') = 'Y'
				and T4."U_EXC_ACTIVO" = 'Y'
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
				lpad(to_bigint(right(trim("NroCtaCargo"),10))+(select sum(to_bigint(right(trim(TX0."NroCtaAbono")
				,case when "TipoCuentaAbono" = 'A' then 11 else 10 end))) from CTE_PROV TX0 where ifnull(TX0."NroCtaAbono",'') <>''),15,'0')||
				--rpad("Filler",100,' ') ||
				--lpad("NroPlanilla",6,'0') ||
				--lpad((select count('A') from CTE_PROV),6,' ') ||
				--lpad('0',6,' ') ||
				--lpad("Estado",80,' ')
				'Z' as "Data"
			from CTE_CAB
			union all 
			select 
				"TipoRegistro"			||
				"TipoCuentaAbono"		||
				rpad(case when :MedPag = 'CG' then '' else "NroCtaAbono" end,20,' ') ||
				"ModalidadDePago"		||
				"TipoDocumentoProv"		||
				rpad("NroDocProv",12,' ')			||
				"CorrDocProv"			||
				rpad(left("NombreProveedor",75),75,' ')		||
				rpad(left("ReferenciaProveedor",40),40,' ')	||
				rpad(left("ReferenciaEmpresa",20),20,' ')	||
				"Moneda"				||
				lpad(round("Importe",2),17,'0')				||
				"Validar"
				||'Z' as "Data"
			from CTE_PROV where ifnull("NroCtaAbono",'') <>'';
		END IF;
	END IF;
END;