DROP FUNCTION if exists public.rsr_f_add_country(int8, text);

CREATE OR REPLACE FUNCTION public.rsr_f_add_country(p_codeid bigint, p_title text)
    RETURNS bigint
    LANGUAGE plpgsql
AS
$function$
declare
    tmp_country_id bigint;
begin
    select c.id
    into tmp_country_id
    from public.country c
    where c.codeid = p_codeid;
    if tmp_country_id is null
    then
        insert into public.country(title, codeid)
        values (p_title, p_codeid)
        returning country.id into tmp_country_id;
    else
        update public.country c
        set title = p_title
        where c.id = tmp_country_id;
    end if;
    return tmp_country_id;
end;
$function$
;

-- Permissions

ALTER FUNCTION public.rsr_f_add_country(int8, text) OWNER TO dbo;
GRANT ALL ON FUNCTION public.rsr_f_add_country(int8, text) TO dbo;