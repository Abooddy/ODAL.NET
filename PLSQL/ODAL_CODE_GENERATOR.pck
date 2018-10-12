CREATE OR REPLACE PACKAGE ODAL_CODE_GENERATOR IS

  -- Author  : Abdullah Adel
  -- Created : 14/4/2018 03:57:00 ?
  -- Purpose : Generates code for both database and application.

  const_owner CONSTANT VARCHAR2(20) := 'YOUR_SCHEMA_USER';

  PROCEDURE repository_main_cg(p_table_name VARCHAR2);
  PROCEDURE procedure_main_cg(p_full_procedure_name VARCHAR2);
  PROCEDURE model_cg(p_table_name VARCHAR2);
  PROCEDURE fields_cg(p_table_name VARCHAR2);
  PROCEDURE repository_class_cg(p_table_name VARCHAR2);
  PROCEDURE repository_interface_cg(p_table_name VARCHAR2);
  PROCEDURE sequence_cg(p_table_name VARCHAR2);
  PROCEDURE procedure_class_cg(p_full_procedure_name VARCHAR2);
  PROCEDURE procedure_interface_cg(p_full_procedure_name VARCHAR2);

END ODAL_CODE_GENERATOR;
/
CREATE OR REPLACE PACKAGE BODY ODAL_CODE_GENERATOR IS

  PROCEDURE repository_main_cg(p_table_name VARCHAR2) IS
  
    v_cnt NUMBER := 0;
  
  BEGIN
    SELECT 1
      INTO v_cnt
      FROM all_tables
     WHERE table_name = upper(p_table_name);
  
    IF v_cnt = 1 THEN
    
      -- Model.
      model_cg(p_table_name);
    
      dbms_output.put_line('----------------------------------------------------------------------');
    
      -- Fields
      fields_cg(p_table_name);
    
      dbms_output.put_line('----------------------------------------------------------------------');
    
      -- Repository class.
      repository_class_cg(p_table_name);
    
      dbms_output.put_line('----------------------------------------------------------------------');
    
      -- Repository interface.
      repository_interface_cg(p_table_name);
    
      dbms_output.put_line('----------------------------------------------------------------------');
    
      -- Sequence creation.
      sequence_cg(p_table_name);
    
    ELSE
      raise_application_error(-20003,
                              'CODE GENERATOR SPECIFIC ERROR! Table does not exist. Please enter a correct table name.');
    END IF;
  END repository_main_cg;

  PROCEDURE procedure_main_cg(p_full_procedure_name VARCHAR2) IS
    v_cnt NUMBER := 0;
  
  BEGIN
    SELECT 1
      INTO v_cnt
      FROM all_procedures
     WHERE object_name || '.' || procedure_name = -- PKG_NAME.PROCEDURE_NAME
           upper(p_full_procedure_name)
       AND owner = const_owner;
  
    IF v_cnt = 1 THEN
    
      -- Procedure class.
      procedure_class_cg(p_full_procedure_name);
    
      dbms_output.put_line('----------------------------------------------------------------------');
    
      -- Procedure interface.
      procedure_interface_cg(p_full_procedure_name);
    
    ELSE
      raise_application_error(-20003,
                              'CODE GENERATOR SPECIFIC ERROR! Procedure does not exist. Please enter a correct procedure name.');
    END IF;
  
  END procedure_main_cg;

  PROCEDURE model_cg(p_table_name VARCHAR2) IS
  
    n VARCHAR2(1) := chr(10); -- New line.
    t VARCHAR2(1) := chr(9); -- Tab.
  
    v_output     CLOB; -- Final result.
    v_header     CLOB;
    v_body       CLOB;
    v_footer     VARCHAR2(20) := t || '}' || n || '}';
    v_get_set    VARCHAR2(20) := '{ get; set; }';
    v_model_name VARCHAR2(50) := REPLACE(initcap(REPLACE(p_table_name,
                                                         '_',
                                                         ' ')),
                                         ' ',
                                         '');
  
    v_using_system    VARCHAR2(20);
    v_model_interface VARCHAR2(20) := 'IModel';
    v_data_type       VARCHAR2(20);
    v_property_name   VARCHAR2(100);
    v_checker         NUMBER;
  
  BEGIN
    -- Get columns.
    FOR rec IN (SELECT *
                  FROM all_tab_columns
                 WHERE owner = const_owner
                   AND table_name = upper(p_table_name)
                 ORDER BY column_id)
    
     LOOP
    
      -- Stops the generator when column name is like '_(number)'. This is not allowed in the framework convention.
      FOR i IN 0 .. 9 LOOP
        -- Returns the postion of the first character of the pattern if it was found in the source string.
        v_checker := instr(rec.column_name, '_' || to_char(i));
      
        -- If pattern was found then throw an exception.
        IF v_checker <> 0 THEN
          raise_application_error(-20000,
                                  'CODE GENERATOR SPECIFIC ERROR! Invalid column name (' ||
                                  rec.column_name ||
                                  '). The "_(number)" format is not allowed in the framework convention. Please consider renaming the column.');
        END IF;
      END LOOP;
    
      IF rec.data_precision >= 19 THEN
        raise_application_error(-20001,
                                'CODE GENERATOR SPECIFIC ERROR! Numbers with precision beyond 19 are not supported.
                              Please consider changing the datatype of column ' ||
                                rec.column_name || '.');
      END IF;
    
      -- At least one column is a DATE.
      IF rec.data_type = 'DATE' THEN
        v_using_system := 'using System;' || n || n;
      END IF;
    
      -- Data type.
      IF rec.data_type = 'VARCHAR2'
         OR rec.data_type = 'NVARCHAR2'
         OR rec.data_type = 'CLOB'
         OR rec.data_type = 'NCLOB'
         OR rec.data_type = 'CHAR'
         OR rec.data_type = 'NCHAR' THEN
        v_data_type := 'string';
      
      ELSIF rec.data_type = 'NUMBER'
            OR rec.data_type = 'FLOAT'
            OR rec.data_type = 'LONG' THEN
        -- C# "int".
        IF (rec.data_precision IS NULL AND rec.data_scale IS NULL) -- NUMBER
           OR (rec.data_precision BETWEEN 0 AND 9 AND rec.data_scale = 0) -- NUMBER(9)
        
         THEN
          v_data_type := 'int?';
        
        ELSIF (rec.data_precision BETWEEN 10 AND 18 AND rec.data_scale = 0) -- NUMBER(16)
         THEN
          v_data_type    := 'Int64?';
          v_using_system := 'using System;' || n || n;
        
          -- C# "double".
        ELSIF (rec.data_precision IS NOT NULL AND rec.data_scale <> 0) -- NUMBER(10,2)
        
         THEN
          v_data_type := 'double?';
        
        END IF;
      
      ELSIF rec.data_type = 'DATE'
            OR rec.data_type = 'TIMESTAMP' THEN
        v_data_type := 'DateTime?';
      ELSE
        -- Rare case, C# Compiler will most likely show this as a syntax error.
        v_data_type := rec.data_type;
      END IF;
    
      -- Convert column names to title case convention. Ex: INSERT_DATE >>> InsertDate
      v_property_name := REPLACE(initcap(REPLACE(rec.column_name, '_', ' ')),
                                 ' ',
                                 '');
    
      -- Construct body.
      v_body := v_body || t || t || 'public ' || v_data_type || ' ' ||
                v_property_name || ' ' || v_get_set || n;
    
      -- Reset variables.
      v_data_type     := NULL;
      v_property_name := NULL;
    
    END LOOP;
  
    -- Construct header.
    v_header := v_using_system || 'namespace OMS.DAL.Models' || n || '{' || n || t ||
                'public class ' || v_model_name || ' : ' ||
                v_model_interface || n || t || '{' || t || t || n;
  
    -- Concatanate header, body and footer.
    v_output := v_output || v_header || v_body || v_footer;
  
    -- Write the result to a file.
    dbms_output.put_line(v_output);
  
  END model_cg;

  PROCEDURE fields_cg(p_table_name VARCHAR2) IS
  
    v_body_constant CONSTANT CLOB := 'namespace OMS.DAL.Fields
{
    public class ##ClassName
    {
        ##ClassBody
    }
}
';
  
    n                      VARCHAR2(1) := chr(10); -- New line.
    t                      VARCHAR2(1) := chr(9); -- Tab.
    v_property_declaration VARCHAR2(100) := 'public const string ##PropertyName = "##PropertyValue"';
  
    -- Body after editing.
    v_body_generated CLOB;
  
    -- Class name.
    v_model_name VARCHAR2(50) := REPLACE(initcap(REPLACE(p_table_name,
                                                         '_',
                                                         ' ')),
                                         ' ',
                                         '') || 'Fields';
  
  BEGIN
  
    dbms_lob.createtemporary(v_body_generated, FALSE);
  
    FOR rec IN (SELECT *
                  FROM all_tab_columns
                 WHERE owner = const_owner
                   AND table_name = upper(p_table_name)
                 ORDER BY column_id)
    
     LOOP
    
      v_body_generated := v_body_generated ||
                          REPLACE(REPLACE(v_property_declaration,
                                          '##PropertyName',
                                          REPLACE(initcap(REPLACE(rec.column_name,
                                                                  '_',
                                                                  ' ')),
                                                  ' ',
                                                  '')),
                                  '##PropertyValue',
                                  rec.column_name) || ';' || n || t || t || t || t;
    
    END LOOP;
  
    dbms_output.put_line(REPLACE(REPLACE(v_body_constant,
                                         '##ClassName',
                                         v_model_name),
                                 '##ClassBody',
                                 v_body_generated));
  
  END fields_cg;

  PROCEDURE repository_class_cg(p_table_name VARCHAR2) AS
  
    -- Class body.
    v_body_constant CONSTANT CLOB := 'using OMS.DAL.Helpers;
using OMS.DAL.Infrastructure;
using OMS.DAL.Models;
using Microsoft.Extensions.Options;

namespace OMS.DAL.Repositories
{
    public class ##ModelNameRepository : BaseRepository, I##ModelNameRepository
    {
        public ##ModelNameRepository(IUnitOfWork unitOfWork, IOptions<DatabaseConfigHelper> configuration) : base(unitOfWork, configuration)
        {

        }

        protected override string GetTableName()
        {
            return DatabaseHelper.ToDbNamingConvention(typeof(##ModelName).Name);
        }

        protected override IModel CreateModel()
        {
            return new ##ModelName();
        }
    }
}';
  
    -- Body after editing.
    v_body_generated CLOB;
  
    -- Model name.
    v_model_name VARCHAR2(50) := REPLACE(initcap(REPLACE(p_table_name,
                                                         '_',
                                                         ' ')),
                                         ' ',
                                         '');
  
    v_cnt NUMBER;
  
  BEGIN
  
    SELECT COUNT(*)
      INTO v_cnt
      FROM all_tables
     WHERE table_name = upper(p_table_name);
  
    IF v_cnt = 1 THEN
    
      -- Insert variable text.
      v_body_generated := REPLACE(v_body_constant,
                                  '##ModelName',
                                  v_model_name);
    
      dbms_output.put_line(v_body_generated);
    
    ELSE
      raise_application_error(-20003,
                              'CODE GENERATOR SPECIFIC ERROR! Table does not exist. Please enter a correct table name.');
    
    END IF;
  END repository_class_cg;

  PROCEDURE repository_interface_cg(p_table_name VARCHAR2) AS
    -- Interface body.
    v_body_constant CONSTANT CLOB := 'namespace OMS.DAL.Repositories
{
    public interface I##ModelNameRepository: IBaseRepository
    {

    }
}
';
  
    -- Body after editing.
    v_body_generated CLOB;
  
    -- Model name.
    v_model_name VARCHAR2(50) := REPLACE(initcap(REPLACE(p_table_name,
                                                         '_',
                                                         ' ')),
                                         ' ',
                                         '');
  
    v_cnt NUMBER;
  BEGIN
  
    SELECT COUNT(*)
      INTO v_cnt
      FROM all_tables
     WHERE table_name = upper(p_table_name);
  
    IF v_cnt = 1 THEN
    
      -- Insert variable text.
      v_body_generated := REPLACE(v_body_constant,
                                  '##ModelName',
                                  v_model_name);
    
      dbms_output.put_line(v_body_generated);
    
    ELSE
      raise_application_error(-20003,
                              'CODE GENERATOR SPECIFIC ERROR! Table does not exist. Please enter a correct table name.');
    
    END IF;
  END repository_interface_cg;

  PROCEDURE sequence_cg(p_table_name VARCHAR2) AS
  
    v_sequence_name VARCHAR2(50);
  
  BEGIN
    v_sequence_name := 'S_' || REPLACE(p_table_name, 'T_', NULL);
  
    EXECUTE IMMEDIATE 'CREATE sequence ' || v_sequence_name ||
                      ' minvalue 1 maxvalue 9999999999999999999999999999 START
    WITH 1 increment BY 1 cache 20';
  
  EXCEPTION
    WHEN OTHERS THEN
      raise_application_error(-20002,
                              'AN ERROR OCCURED IN CREATING THE SEQUENCE! ' ||
                              chr(10) || SQLERRM);
  END sequence_cg;

  PROCEDURE procedure_class_cg(p_full_procedure_name VARCHAR2) IS
    v_class_body CLOB := 'using System.Collections.Generic;
using System.Data;
using OMS.DAL.Helpers;
using OMS.DAL.Infrastructure;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;

namespace OMS.DAL.Procedures
{
    public class ##PROCEDURE_NAMEProcedure : BaseProcedure, I##PROCEDURE_NAMEProcedure
    {
    ##PROCEDURE_PARAMETERS
        private readonly DatabaseConfigHelper configuration;

        public ##PROCEDURE_NAMEProcedure(IUnitOfWork unitOfWork, IOptions<DatabaseConfigHelper> configuration) : base(unitOfWork, configuration)
        {
            this.configuration = configuration.Value;
        }

        protected override string GetProcedureName()
        {
            return "##PROCEDURE_FULL_NAME";
        }

        protected override IEnumerable<OracleParameter> GetProcedureParameters()
        {
            List<OracleParameter> parameterCollection = new List<OracleParameter>();

            foreach (var parameter in this.GetType().GetProperties())
            {
                   parameterCollection.Add(new OracleParameter(
                    DatabaseHelper.ToDbNamingConvention(parameter.Name),
                    DatabaseHelper.ToOraclDbType(parameter.PropertyType),
                    DatabaseHelper.GetParameterSize(parameter.PropertyType, configuration),
                    parameter.GetValue(this),
                    ParameterDirection.Input));
            }

            // ADD ADDITIONAL CUSTOM PARAMETERS HERE! STILL NOT RECOMMENDED.

            return parameterCollection;
        }
    }
}';
  
    n VARCHAR2(1) := chr(10); -- New line.
    t VARCHAR2(1) := chr(9); -- Tab.
  
    v_cnt                 NUMBER := 0;
    v_procedure_name      VARCHAR2(50);
    v_package_name        VARCHAR2(50);
    v_procedure_full_name VARCHAR2(50);
    v_body                CLOB;
  
    v_using_system  VARCHAR2(20);
    v_get_set       VARCHAR2(20) := '{ get; set; }';
    v_checker       NUMBER;
    v_data_type     VARCHAR2(50);
    v_property_name VARCHAR2(50);
  
  BEGIN
    SELECT 1
      INTO v_cnt
      FROM all_procedures
     WHERE object_name || '.' || procedure_name = -- PKG_NAME.PROCEDURE_NAME
           upper(p_full_procedure_name);
  
    IF v_cnt = 1 THEN
    
      SELECT object_name, procedure_name
        INTO v_package_name, v_procedure_name
        FROM all_procedures
       WHERE object_name || '.' || procedure_name =
             upper(p_full_procedure_name);
    
      v_procedure_full_name := v_package_name || '.' || v_procedure_name; -- PKG_NAME.PROCEDURE_NAME -- Too lazy to split the input parameter.
    
      FOR rec IN (SELECT *
                    FROM all_arguments
                   WHERE owner = const_owner
                     AND object_name = v_procedure_name
                     AND package_name = v_package_name
                     AND in_out = 'IN'
                   ORDER BY position) LOOP
      
        -- Stops the generator when parameter name is like '_(number)'. This is not allowed in the framework convention.
        FOR i IN 0 .. 9 LOOP
          -- Returns the postion of the first character of the pattern if it was found in the source string.
          v_checker := instr(rec.argument_name, '_' || to_char(i));
        
          -- If pattern was found then throw an exception.
          IF v_checker <> 0 THEN
            raise_application_error(-20000,
                                    'CODE GENERATOR SPECIFIC ERROR! Invalid parameter name (' ||
                                    rec.argument_name ||
                                    '). The "_(number)" format is not allowed in the framework convention. Please consider renaming the parameter.');
          END IF;
        END LOOP;
      
        -- At least one column is a DATE.
        IF rec.data_type = 'DATE' THEN
          v_using_system := 'using System;' || n;
        END IF;
      
        -- Data type.
        IF rec.data_type = 'VARCHAR2'
           OR rec.data_type = 'NVARCHAR2'
           OR rec.data_type = 'CLOB'
           OR rec.data_type = 'NCLOB'
           OR rec.data_type = 'CHAR'
           OR rec.data_type = 'NCHAR' THEN
          v_data_type := 'string';
        
        ELSIF rec.data_type = 'NUMBER'
              OR rec.data_type = 'FLOAT'
              OR rec.data_type = 'LONG'
              OR rec.data_type = 'PLS_INTEGER'
              OR rec.data_type = 'BINARY_INTEGER' THEN
          v_data_type := 'int?';
        
        ELSIF rec.data_type = 'DATE'
              OR rec.data_type = 'TIMESTAMP' THEN
          v_data_type := 'DateTime?';
        
        ELSIF rec.data_type = 'PL/SQL BOOLEAN' THEN
          v_data_type := 'bool?';
        ELSE
          -- Rare case, C# Compiler will most likely show this as a syntax error.
          v_data_type := rec.data_type;
        END IF;
      
        -- Convert column names to title case convention. Ex: INSERT_DATE >>> InsertDate
        v_property_name := REPLACE(initcap(REPLACE(rec.argument_name,
                                                   '_',
                                                   ' ')),
                                   ' ',
                                   '');
      
        -- Construct body.
        v_body := v_body || t || t || 'public ' || v_data_type || ' ' ||
                  v_property_name || ' ' || v_get_set || n || t || t;
      
        -- Reset variables.
        v_data_type     := NULL;
        v_property_name := NULL;
      
      END LOOP;
      ----------------------------------------------------------------------------------------------------------------------
    
      v_procedure_name := REPLACE(initcap(REPLACE(v_procedure_name,
                                                  '_',
                                                  ' ')),
                                  ' ',
                                  '');
    
      -- Replace variable text.
      v_class_body := REPLACE(v_class_body,
                              '##PROCEDURE_NAME',
                              v_procedure_name);
    
      v_class_body := REPLACE(v_class_body,
                              '##PROCEDURE_FULL_NAME',
                              v_procedure_full_name);
    
      v_class_body := REPLACE(v_class_body,
                              '##PROCEDURE_PARAMETERS',
                              v_body);
    
      IF v_using_system IS NOT NULL THEN
        v_class_body := v_using_system || v_class_body;
      END IF;
    
      dbms_output.put_line(v_class_body);
    
    ELSE
      raise_application_error(-20004,
                              'CODE GENERATOR SPECIFIC ERROR! Procedure does not exist. Please recheck the procedure name.');
    END IF;
  END procedure_class_cg;

  PROCEDURE procedure_interface_cg(p_full_procedure_name VARCHAR2) IS
    -- Interface body.
    v_body_constant CLOB := 'namespace OMS.DAL.Procedures
{
    public interface I##PROCEDURE_NAMEProcedure: IBaseProcedure
    {
    ##PROCEDURE_PARAMETERS
    }
}
';
  
    -- Procedure name.
    v_procedure_name VARCHAR2(50) := REPLACE(initcap(REPLACE(substr(p_full_procedure_name,
                                                                    instr(p_full_procedure_name,
                                                                          '.') + 1,
                                                                    length(p_full_procedure_name)),
                                                             '_',
                                                             ' ')),
                                             ' ',
                                             '');
    n                VARCHAR2(1) := chr(10); -- New line.
    t                VARCHAR2(1) := chr(9); -- Tab.
    v_using_system   VARCHAR2(20);
    v_get_set        VARCHAR2(20) := '{ get; set; }';
    v_checker        NUMBER;
    v_data_type      VARCHAR2(50);
    v_property_name  VARCHAR2(50);
    v_body           CLOB;
  
    v_cnt NUMBER;
  BEGIN
  
    SELECT 1
      INTO v_cnt
      FROM all_procedures
     WHERE object_name || '.' || procedure_name = -- PKG_NAME.PROCEDURE_NAME
           upper(p_full_procedure_name)
       AND owner = const_owner;
  
    IF v_cnt = 1 THEN
      FOR rec IN (SELECT *
                    FROM all_arguments
                   WHERE owner = const_owner
                     AND object_name =
                         upper(substr(p_full_procedure_name,
                                      instr(p_full_procedure_name, '.') + 1,
                                      length(p_full_procedure_name)))
                     AND package_name =
                         upper(substr(p_full_procedure_name,
                                      0,
                                      instr(p_full_procedure_name, '.') - 1))
                     AND in_out = 'IN'
                   ORDER BY position) LOOP
      
        -- Stops the generator when parameter name is like '_(number)'. This is not allowed in the framework convention.
        FOR i IN 0 .. 9 LOOP
          -- Returns the postion of the first character of the pattern if it was found in the source string.
          v_checker := instr(rec.argument_name, '_' || to_char(i));
        
          -- If pattern was found then throw an exception.
          IF v_checker <> 0 THEN
            raise_application_error(-20000,
                                    'CODE GENERATOR SPECIFIC ERROR! Invalid parameter name (' ||
                                    rec.argument_name ||
                                    '). The "_(number)" format is not allowed in the framework convention. Please consider renaming the parameter.');
          END IF;
        END LOOP;
      
        -- At least one column is a DATE.
        IF rec.data_type = 'DATE' THEN
          v_using_system := 'using System;' || n || n;
        END IF;
      
        -- Data type.
        IF rec.data_type = 'VARCHAR2'
           OR rec.data_type = 'NVARCHAR2'
           OR rec.data_type = 'CLOB'
           OR rec.data_type = 'NCLOB'
           OR rec.data_type = 'CHAR'
           OR rec.data_type = 'NCHAR' THEN
          v_data_type := 'string';
        
        ELSIF rec.data_type = 'NUMBER'
              OR rec.data_type = 'FLOAT'
              OR rec.data_type = 'LONG'
              OR rec.data_type = 'PLS_INTEGER'
              OR rec.data_type = 'BINARY_INTEGER' THEN
          v_data_type := 'int?';
        
        ELSIF rec.data_type = 'DATE'
              OR rec.data_type = 'TIMESTAMP' THEN
          v_data_type := 'DateTime?';
        
        ELSIF rec.data_type = 'PL/SQL BOOLEAN' THEN
          v_data_type := 'bool?';
        ELSE
          -- Rare case, C# Compiler will most likely show this as a syntax error.
          v_data_type := rec.data_type;
        END IF;
      
        -- Convert column names to title case convention. Ex: INSERT_DATE >>> InsertDate
        v_property_name := REPLACE(initcap(REPLACE(rec.argument_name,
                                                   '_',
                                                   ' ')),
                                   ' ',
                                   '');
      
        -- Construct body.
        v_body := v_body || t || t || v_data_type || ' ' || v_property_name || ' ' ||
                  v_get_set || n || t || t;
      
        -- Reset variables.
        v_data_type     := NULL;
        v_property_name := NULL;
      
      END LOOP;
    
      -- Insert variable text.
      v_body_constant := REPLACE(v_body_constant,
                                 '##PROCEDURE_NAME',
                                 v_procedure_name);
    
      v_body_constant := REPLACE(v_body_constant,
                                 '##PROCEDURE_PARAMETERS',
                                 v_body);
      IF v_using_system IS NOT NULL THEN
        v_body_constant := v_using_system || v_body_constant;
      END IF;
    
      dbms_output.put_line(v_body_constant);
    
    ELSE
      raise_application_error(-20003,
                              'CODE GENERATOR SPECIFIC ERROR! Procedure does not exist. Please enter a correct procedure name.');
    
    END IF;
  END;

END ODAL_CODE_GENERATOR;
/
