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
	
	public partial class UpdateObject : ISpecificRecord
	{
		public static Schema _SCHEMA = Schema.Parse(@"{""type"":""record"",""name"":""UpdateObject"",""namespace"":""eu.driver.model.worldexplorer"",""fields"":[{""name"":""User"",""type"":""string""},{""name"":""Name"",""type"":""string""},{""name"":""posX"",""type"":""float""},{""name"":""posY"",""type"":""float""},{""name"":""posZ"",""type"":""float""},{""name"":""lat"",""type"":""double""},{""name"":""lon"",""type"":""double""},{""name"":""scaleX"",""type"":""float""},{""name"":""scaleY"",""type"":""float""},{""name"":""scaleZ"",""type"":""float""},{""name"":""rotX"",""type"":""float""},{""name"":""rotY"",""type"":""float""},{""name"":""rotZ"",""type"":""float""},{""name"":""centerPosX"",""type"":""float""},{""name"":""centerPosY"",""type"":""float""},{""name"":""centerPosZ"",""type"":""float""}]}");
		private string _User;
		private string _Name;
		private float _posX;
		private float _posY;
		private float _posZ;
		private double _lat;
		private double _lon;
		private float _scaleX;
		private float _scaleY;
		private float _scaleZ;
		private float _rotX;
		private float _rotY;
		private float _rotZ;
		private float _centerPosX;
		private float _centerPosY;
		private float _centerPosZ;
		public virtual Schema Schema
		{
			get
			{
				return UpdateObject._SCHEMA;
			}
		}
		public string User
		{
			get
			{
				return this._User;
			}
			set
			{
				this._User = value;
			}
		}
		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				this._Name = value;
			}
		}
		public float posX
		{
			get
			{
				return this._posX;
			}
			set
			{
				this._posX = value;
			}
		}
		public float posY
		{
			get
			{
				return this._posY;
			}
			set
			{
				this._posY = value;
			}
		}
		public float posZ
		{
			get
			{
				return this._posZ;
			}
			set
			{
				this._posZ = value;
			}
		}
		public double lat
		{
			get
			{
				return this._lat;
			}
			set
			{
				this._lat = value;
			}
		}
		public double lon
		{
			get
			{
				return this._lon;
			}
			set
			{
				this._lon = value;
			}
		}
		public float scaleX
		{
			get
			{
				return this._scaleX;
			}
			set
			{
				this._scaleX = value;
			}
		}
		public float scaleY
		{
			get
			{
				return this._scaleY;
			}
			set
			{
				this._scaleY = value;
			}
		}
		public float scaleZ
		{
			get
			{
				return this._scaleZ;
			}
			set
			{
				this._scaleZ = value;
			}
		}
		public float rotX
		{
			get
			{
				return this._rotX;
			}
			set
			{
				this._rotX = value;
			}
		}
		public float rotY
		{
			get
			{
				return this._rotY;
			}
			set
			{
				this._rotY = value;
			}
		}
		public float rotZ
		{
			get
			{
				return this._rotZ;
			}
			set
			{
				this._rotZ = value;
			}
		}
		public float centerPosX
		{
			get
			{
				return this._centerPosX;
			}
			set
			{
				this._centerPosX = value;
			}
		}
		public float centerPosY
		{
			get
			{
				return this._centerPosY;
			}
			set
			{
				this._centerPosY = value;
			}
		}
		public float centerPosZ
		{
			get
			{
				return this._centerPosZ;
			}
			set
			{
				this._centerPosZ = value;
			}
		}
		public virtual object Get(int fieldPos)
		{
			switch (fieldPos)
			{
			case 0: return this.User;
			case 1: return this.Name;
			case 2: return this.posX;
			case 3: return this.posY;
			case 4: return this.posZ;
			case 5: return this.lat;
			case 6: return this.lon;
			case 7: return this.scaleX;
			case 8: return this.scaleY;
			case 9: return this.scaleZ;
			case 10: return this.rotX;
			case 11: return this.rotY;
			case 12: return this.rotZ;
			case 13: return this.centerPosX;
			case 14: return this.centerPosY;
			case 15: return this.centerPosZ;
			default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Get()");
			};
		}
		public virtual void Put(int fieldPos, object fieldValue)
		{
			switch (fieldPos)
			{
			case 0: this.User = (System.String)fieldValue; break;
			case 1: this.Name = (System.String)fieldValue; break;
			case 2: this.posX = (System.Single)fieldValue; break;
			case 3: this.posY = (System.Single)fieldValue; break;
			case 4: this.posZ = (System.Single)fieldValue; break;
			case 5: this.lat = (System.Double)fieldValue; break;
			case 6: this.lon = (System.Double)fieldValue; break;
			case 7: this.scaleX = (System.Single)fieldValue; break;
			case 8: this.scaleY = (System.Single)fieldValue; break;
			case 9: this.scaleZ = (System.Single)fieldValue; break;
			case 10: this.rotX = (System.Single)fieldValue; break;
			case 11: this.rotY = (System.Single)fieldValue; break;
			case 12: this.rotZ = (System.Single)fieldValue; break;
			case 13: this.centerPosX = (System.Single)fieldValue; break;
			case 14: this.centerPosY = (System.Single)fieldValue; break;
			case 15: this.centerPosZ = (System.Single)fieldValue; break;
			default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Put()");
			};
		}
	}
}
