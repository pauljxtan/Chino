CREATE TABLE IF NOT EXISTS images (
    image_name TEXT PRIMARY KEY
);

CREATE TABLE IF NOT EXISTS tags (
    tag_name TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS imagetags (
    image_name TEXT NOT NULL,
	tag_name TEXT NOT NULL,
	FOREIGN KEY(image_name) REFERENCES image(image_name),
	FOREIGN KEY(tag_name) REFERENCES tags(tag_name)
);
