DROP FUNCTION if exists public.rsr_f_add_settlement(int8, text);

CREATE OR REPLACE FUNCTION public.rsr_f_add_settlement(p_code_id bigint, p_title text)
    RETURNS bigint
    LANGUAGE plpgsql
AS
$function$
declare
    tmp_settlement_id bigint;
begin
    select s.id into tmp_settlement_id from public.settlement s where s.codeid = p_code_id;
    if tmp_settlement_id is null
    then
        insert into public.settlement (title, codeid)
        values (p_title, p_code_id)
        returning settlement.id into tmp_settlement_id;
    else
        update public.settlement s
        set title = p_title
        where s.id = tmp_settlement_id;
    end if;
    return tmp_settlement_id;
end;
$function$
;

-- Permissions

ALTER FUNCTION public.rsr_f_add_settlement(int8, text) OWNER TO dbo;
GRANT ALL ON FUNCTION public.rsr_f_add_settlement(int8, text) TO dbo;