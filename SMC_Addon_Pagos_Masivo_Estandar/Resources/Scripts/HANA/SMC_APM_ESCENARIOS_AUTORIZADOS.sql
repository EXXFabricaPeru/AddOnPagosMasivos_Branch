CREATE procedure "SMC_APM_ESCENARIOS_AUTORIZADOS"
(CODIGO INT)
AS
BEGIN

select "U_SMC_ACCION" "Accion","U_SMC_ACCION2" "Accion2" from "@SMC_APM_AUTORIZAR" 
where "U_SMC_COD_ESCENARIO" = :CODIGO;

END