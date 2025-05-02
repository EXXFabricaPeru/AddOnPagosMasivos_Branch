CREATE FUNCTION EXD_FN_H2H_TXT_SCOTIABANK_DIGITO_CONTROL
(
	fechaOrden int,
	montoPago int,
	moneda int,
	formaPago int,
	cuentaAbono int,
	cuentaCargo int
	
)
RETURNS RSLT INT
AS
BEGIN
	declare nroConvenio int = 1911;	
	declare moduloRaiz int = 77;
	declare sumatoria int = :nroConvenio + :fechaOrden + :montoPago + :moneda + :formaPago + :cuentaAbono + :cuentaCargo;
	declare residuo int = 0;
	
	sumatoria := rpad(:sumatoria,9,0);
	sumatoria := 	substring(:sumatoria,1,1) * 1 +
					substring(:sumatoria,2,1) * 2 +
					substring(:sumatoria,3,1) * 3 +
					substring(:sumatoria,4,1) * 4 +
					substring(:sumatoria,5,1) * 5 +
					substring(:sumatoria,6,1) * 6 +
					substring(:sumatoria,7,1) * 7 +
					substring(:sumatoria,8,1) * 8 +
					substring(:sumatoria,9,1) * 9;																									
	residuo := mod(:sumatoria,:moduloRaiz);
	RSLT := :moduloRaiz - :residuo;
END;