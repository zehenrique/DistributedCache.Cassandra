CREATE TABLE IF NOT EXISTS cassandra_cache (
	id text,
	value blob,
	expiration_time timestamp,
    PRIMARY KEY (id)
) WITH gc_grace_seconds = 172800;