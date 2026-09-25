using System;
using System.Text;

[Serializable]
public class VolumeCloudsWeatherLayerConfig
{
	public VolumeCloudsNoiseLayerConfig Instability = new VolumeCloudsNoiseLayerConfig();

	public VolumeCloudsNoiseLayerConfig CoverageBase = new VolumeCloudsNoiseLayerConfig();

	public VolumeCloudsNoiseLayerConfig CoverageDetailPerlin = new VolumeCloudsNoiseLayerConfig();

	public VolumeCloudsNoiseLayerConfig CoverageDetailWorley = new VolumeCloudsNoiseLayerConfig();

	public VolumeCloudsCurlNoiseConfig Curl = new VolumeCloudsCurlNoiseConfig();

	public void CopyFrom(VolumeCloudsWeatherLayerConfig copy)
	{
		Instability.CopyFrom(copy.Instability);
		CoverageBase.CopyFrom(copy.CoverageBase);
		CoverageDetailPerlin.CopyFrom(copy.CoverageDetailPerlin);
		CoverageDetailWorley.CopyFrom(copy.CoverageDetailWorley);
		Curl.CopyFrom(copy.Curl);
	}

	public void Output(StringBuilder sb, VolumeClouds.NoiseOffsets ofs)
	{
		sb.AppendLine("Cov base:");
		CoverageBase.Output(sb, ofs.CoverageBase);
		sb.AppendLine("Cov dp:");
		CoverageDetailPerlin.Output(sb, ofs.CoverageDetailPerlin);
		sb.AppendLine("Cov dw:");
		CoverageDetailWorley.Output(sb, ofs.CoverageDetailWorley);
	}
}
