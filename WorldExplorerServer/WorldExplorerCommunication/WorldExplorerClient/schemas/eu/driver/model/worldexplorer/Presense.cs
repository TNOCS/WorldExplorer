// ------------------------------------------------------------------------------
// <auto-generated>
//    Generated by avrogen, version 1.7.7.5
//    Changes to this file may cause incorrect behavior and will be lost if code
//    is regenerated
// </auto-generated>
// ------------------------------------------------------------------------------
namespace eu.driver.model.worldexplorer
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using global::Avro;
	using global::Avro.Specific;
    using Newtonsoft.Json;

    public partial class Presense : ISpecificRecord
	{
		public static Schema _SCHEMA = Schema.Parse(@"{""type"":""record"",""name"":""Presense"",""namespace"":""eu.driver.model.worldexplorer"",""fields"":[{""name"":""id"",""type"":""string""},{""name"":""name"",""type"":""string""},{""name"":""xpos"",""type"":""float""},{""name"":""ypos"",""type"":""float""},{""name"":""zpos"",""type"":""float""},{""name"":""xrot"",""type"":""float""},{""name"":""yrot"",""type"":""float""},{""name"":""zrot"",""type"":""float""},{""name"":""r"",""type"":""int""},{""name"":""g"",""type"":""int""},{""name"":""b"",""type"":""int""}]}");
		private string _id;
		private string _name;
		private float _xpos;
		private float _ypos;
		private float _zpos;
		private float _xrot;
		private float _yrot;
		private float _zrot;
		private int _r;
		private int _g;
		private int _b;

		public virtual Schema Schema
		{
			get
			{
				return Presense._SCHEMA;
			}
		}
		public string id
		{
			get
			{
				return this._id;
			}
			set
			{
				this._id = value;
			}
		}
		public string name
		{
			get
			{
				return this._name;
			}
			set
			{
				this._name = value;
			}
		}
		public float xpos
		{
			get
			{
				return this._xpos;
			}
			set
			{
				this._xpos = value;
			}
		}
		public float ypos
		{
			get
			{
				return this._ypos;
			}
			set
			{
				this._ypos = value;
			}
		}
		public float zpos
		{
			get
			{
				return this._zpos;
			}
			set
			{
				this._zpos = value;
			}
		}
		public float xrot
		{
			get
			{
				return this._xrot;
			}
			set
			{
				this._xrot = value;
			}
		}
		public float yrot
		{
			get
			{
				return this._yrot;
			}
			set
			{
				this._yrot = value;
			}
		}
		public float zrot
		{
			get
			{
				return this._zrot;
			}
			set
			{
				this._zrot = value;
			}
		}
		public int r
		{
			get
			{
				return this._r;
			}
			set
			{
				this._r = value;
			}
		}
		public int g
		{
			get
			{
				return this._g;
			}
			set
			{
				this._g = value;
			}
		}
		public int b
		{
			get
			{
				return this._b;
			}
			set
			{
				this._b = value;
			}
		}
		public virtual object Get(int fieldPos)
		{
			switch (fieldPos)
			{
			case 0: return this.id;
			case 1: return this.name;
			case 2: return this.xpos;
			case 3: return this.ypos;
			case 4: return this.zpos;
			case 5: return this.xrot;
			case 6: return this.yrot;
			case 7: return this.zrot;
			case 8: return this.r;
			case 9: return this.g;
			case 10: return this.b;
			default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Get()");
			};
		}
		public virtual void Put(int fieldPos, object fieldValue)
		{
			switch (fieldPos)
			{
			case 0: this.id = (System.String)fieldValue; break;
			case 1: this.name = (System.String)fieldValue; break;
			case 2: this.xpos = (System.Single)fieldValue; break;
			case 3: this.ypos = (System.Single)fieldValue; break;
			case 4: this.zpos = (System.Single)fieldValue; break;
			case 5: this.xrot = (System.Single)fieldValue; break;
			case 6: this.yrot = (System.Single)fieldValue; break;
			case 7: this.zrot = (System.Single)fieldValue; break;
			case 8: this.r = (System.Int32)fieldValue; break;
			case 9: this.g = (System.Int32)fieldValue; break;
			case 10: this.b = (System.Int32)fieldValue; break;
			default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Put()");
			};
		}
	}
}
