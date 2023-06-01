/* PGP: Symmetric */
--Encrypt
CREATE OR REPLACE FUNCTION emro_sym_encrypt(t text, secret text,algotype text) RETURNS bytea AS $$
BEGIN
   RETURN pgp_sym_encrypt(t, secret,algotype);
END;
$$ LANGUAGE plpgsql;
                          

--Decrypt
CREATE OR REPLACE FUNCTION emro_sym_decrypt(t bytea, secret text,algotype text) RETURNS text AS $$
BEGIN
   RETURN pgp_sym_decrypt(t, secret,algotype);
END;
$$ LANGUAGE plpgsql;