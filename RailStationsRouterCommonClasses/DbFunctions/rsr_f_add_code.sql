DROP FUNCTION if exists public.rsr_f_add_code(text, text);

CREATE OR REPLACE FUNCTION public.rsr_f_add_code(p_yandex_code text, p_esr_code text)
    RETURNS bigint
    LANGUAGE plpgsql
AS
$function$
declare
    tmp_out_id bigint;
begin

    select c.id
    into tmp_out_id
    from public.codes c
    where coalesce(c.esr_code, '') = coalesce(p_esr_code, '')
      and coalesce(c.yandex_code, '') = coalesce(p_yandex_code, '');
    if tmp_out_id is null
    then
        insert into codes (yandex_code, esr_code)
        select p_yandex_code, p_esr_code
        where not exists(select null
                         from codes as c
                         where coalesce(c.yandex_code, '') = coalesce(p_yandex_code, '')
                           and coalesce(c.esr_code, '') = coalesce(p_esr_code, ''))
        returning id into tmp_out_id;
    end if;
    return tmp_out_id;
end;
$function$
;

-- Permissions

ALTER FUNCTION public.rsr_f_add_code(text, text) OWNER TO dbo;
GRANT ALL ON FUNCTION public.rsr_f_add_code(text, text) TO dbo;