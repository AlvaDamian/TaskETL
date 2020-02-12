## TaskETL
An ETL processor based on C# Tasks.

## WorkFlow
TaskETL will execute ETL jobs concurrently. Each job will be executed by a proccessor.

In TaskETL, an ETL job is divided into 3 parts:
 - An extractor: where data comes from.
 - A transformer: transform data from source type to destination type.
 - A loader: where data will be "stored".

The processor will take data from the extractor, pass it to the transformer. Finally, data produced by trasformer will be supplied to the loader.
If there is an excepction in any of these components (extractor, transformer or loader) it will be catched and returned as a failed job result.

## Short example
### Create an extractor
Data has to be extracted from source. To do that, an extractor has to be implemented.
An extractor implements **IExtractor\<SourceType\>** interface, where **SourceData** is source data type.
For example, this extractor will take strings from somewhere. Each string represents an item in the form **code - name**. Code is a number, name a string and they are separated by a **-**. For simplicity, data was hardcoded.

    using System.Collections.Generic;
    using TaskETL.Extractors;

    namespace Example
    {
      public class ItemExtractor : IExtractor<IEnumerable<string>>
      {
        public IEnumerable<string> Extract()
        {
          ICollection<string> ret = new List<string>();

          ret.Add("456 - Pool");
          ret.Add("99-Application");
          ret.Add("7474-     CellPhone");

          return ret;
        }

        public string GetID()
        {
          return "Extractor ID";
        }
      }
    }

### Create a transformer
A transformer will take data from a extractor and convert it to a data type supported by a loader. To implement a transformer, an object has to implement the interface **ITransformer<SourceType, DestinationType>**, where:

 - **SourceType** is the data type provided by an extractor.
 - **DestinationType** is the data type expected by a loader.

In this example, an extractor will take items saved in a string in the format **code - name** and convert them to an **Item** object.

    using System;
    using System.Collections.Generic;
    using TaskETL.Transformers;

    namespace Example
    {
      public class ItemTransformer : ITransformer<IEnumerable<string>, IEnumerable<Item>>
      {
        public string GetID()
        {
          return "Transformer ID";
        }

        public IEnumerable<Item> transform(IEnumerable<string> source)
        {
          ICollection<Item> ret = new List<Item>();

          foreach (var item in source)
          {
            int separatorPosition = item.IndexOf('-');

            int id = Int32.Parse(item.Substring(0, separatorPosition).Trim());
            string name = item.Substring(separatorPosition + 1).Trim();

            ret.Add(new Item(id, name));
          }

          return ret;
        }
      }
    }

#### Item model
    public class Item
    {
      public int Code { get; private set; }
      public string Name { get; private set; }

      public Item(int code, string name)
      {
        this.Code = code;
        this.Name = name.Trim();
      }

      public override string ToString()
      {
        string ret = 
                "Item{\"code\":" +
                this.Code + 
                ",\"name\":\"" + 
                this.Name + 
                "\"}";

        return ret;
      }
    }

### Create a loader
A loader will take data transformed and do something with it. A loader has to implementa interface **ILoader\<DestinationType\>**, where **DestinationType** is the data type expected by the loader.

In this example, the loader will accept an **IEnumerable\<Item\>** and log in console each of them.

    using System;
    using System.Collections.Generic;
    using TaskETL.Loaders;

    namespace Example
    {
      public class ItemLoader : ILoader<IEnumerable<Item>>
      {
        public string GetID()
        {
          return "Item Loader";
        }

        public void load(IEnumerable<Item> data)
        {
          foreach (var item in data)
          {
            Console.WriteLine(item.ToString());
          }
        }
      }
    }

### Create a processor
A processor will take all **IExtractor**, use the return type in their respective **ITransformer** and send each result to al **ILoader**.
A processor implementes interface **IProcessor**. To create a processor, use object **ProcessorBuilder**

    //Extractor declared above
    IExtractor<IEnumerable<string>> extractor = new ItemExtractor();
    
    //Transformer declared above
    ITransformer<IEnumerable<string>, IEnumerable<Item>> transformer = new ItemTransformer();
    
    //Loader declared above
    ILoader<IEnumerable<Item>> loader = new ItemLoader();

    IProcessor procesor =
                new ProcessorBuilder<IEnumerable<Item>>(loader)
                .AddSource("Processor ID", extractor, transformer)
                .build();

### Execute
After a processor has been built, it can be executed. This will generate an **IEnumerable\<Task\<JobResult\>\>**.

    //Process method will create and execute each task
    IEnumerable<Task<JobResult>> tasks = procesor.Process();

    //Wait for all tasks to finnish.
    Task.WaitAll(tasks.ToArray());

    //Analize results
    foreach (var item in tasks)
    {
      JobResult result = item.Result;

      if (!result.CompletedWithouErrors)
      {
        Console.Error.WriteLine("Error in ETL job.");
        foreach (var error in result.Errors)
        {
          Console.WriteLine(error);
        }
      }
      else
      {
        Console.Out.WriteLine("Job completed without errors");
      }
    }
    Console.ReadLine();

### Output
    Item{"code":456,"name":"Pool"}
    Item{"code":99,"name":"Application"}
    Item{"code"7474,"name":"CellPhone"}
    Job completed without errors
