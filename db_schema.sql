USE recipesdb
CREATE TABLE Recipes(
	Recipe_Id INT UNIQUE NOT NULL AUTO_INCREMENT,
    TitleShort varchar(50) NOT NULL ,
    TitleLong varchar(150),
    Description varchar(2000) NOT NULL ,
    OriginalLink varchar(500),
    CreationDate TIMESTAMP DEFAULT NOW();
    AuditDate TIMESTAMP DEFAULT NOW();
    LastModifier varchar(150) NOT NULL;
    PRIMARY KEY (Recipe_Id)
);
    
CREATE TABLE units(
	Unit_Id INT UNIQUE NOT NULL AUTO_INCREMENT,
    Name varchar(50) NOT NULL,
    Symbol varchar(15) NOT NULL,
	Constraint UC_Units UNIQUE (Name,Symbol),
	PRIMARY KEY (Unit_Id)
);

CREATE TABLE categories(
	Category_Id INT UNIQUE NOT NULL AUTO_INCREMENT,
    Name varchar(100) UNIQUE NOT NULL,
    Description varchar(300) NOT NULL,
    PRIMARY KEY (Category_Id)
);

CREATE TABLE recipe_ingredients(
	RecipeIng_Id INT UNIQUE NOT NULL AUTO_INCREMENT,
    Name varchar(100) NOT NULL,
    Quantity decimal NOT NULL,
    Recipe_Id INT,
    Unit_Id INT,
    PRIMARY KEY (RecipeIng_Id),
    FOREIGN KEY (Recipe_Id) REFERENCES recipes(Recipe_Id) ON DELETE CASCADE,
    FOREIGN KEY (Unit_Id) REFERENCES units(Unit_Id) ON DELETE CASCADE
);

CREATE TABLE recipe_images(
	RecipeImg_Id INT UNIQUE NOT NULL AUTO_INCREMENT,
    Image_path varchar(200) NOT NULL,
    Title varchar(200) NOT NULL,
    Recipe_Id INT,
    PRIMARY KEY (RecipeImg_Id),
    FOREIGN KEY (Recipe_Id) REFERENCES recipes(Recipe_Id) ON DELETE CASCADE
);

CREATE TABLE recipe_categories(
	RecipeCat_Id INT UNIQUE NOT NULL AUTO_INCREMENT,
    Recipe_Id INT,
    Category_Id INT,
    PRIMARY KEY (RecipeCat_Id),
    FOREIGN KEY (Recipe_Id) REFERENCES recipes(Recipe_Id) ON DELETE CASCADE,
    FOREIGN KEY (Category_Id) REFERENCES categories(Category_Id) ON DELETE CASCADE
);

CREATE TABLE recipe_instructions(
	RecipeInst_Id INT UNIQUE NOT NULL AUTO_INCREMENT,
    StepNum INT NOT NULL,
    Instruction varchar(500) NOT NULL,
    Recipe_Id INT,
    PRIMARY KEY (RecipeInst_Id),
    FOREIGN KEY (Recipe_Id) REFERENCES Recipes(Recipe_Id) ON DELETE CASCADE
);