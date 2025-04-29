using System.IO;
using UnityEngine;

public class SaveManager{
    public string saveName = "Test";
    string savePath;
    public void Save(){
        savePath = PathLoader.GetSavePath(saveName);
        SaveWorld("main_world");
    }

    public void SaveWorld(string worldName){
        string worldPath = PathLoader.SplicingAndCreatePath(PathLoader.GetWorldsPath(saveName), worldName);
    }

    public void SaveChunk(string worldName,int x, int y){
        string chunksPath = PathLoader.GetChunksPath(saveName, worldName);
        string chunkFileNmae = $"{x},{y}.dat";
    }

    public void SaveChunkToBinary(ChunkEntity chunk, string worldName = "main_world")
    {
        string chunksPath = PathLoader.GetChunksPath(saveName, worldName);
        string chunkFileNmae = $"{chunk.chunkCoord.x},{chunk.chunkCoord.x}.dat";
        string chunkFilePath = Path.Combine(chunksPath, chunkFileNmae);
        BlockEntity[,,] blocks = chunk.blocks;
        using (var writer = new BinaryWriter(File.Open(chunkFilePath, FileMode.Create)))
        {
            chunk.ExecuteAll((block)=>{
                if(block is not null){
                    writer.Write(block.blockCoordInChunk.x);
                    writer.Write(block.blockCoordInChunk.y);
                    writer.Write(block.blockCoordInChunk.z);
                    writer.Write(block.ToData());
                }
            });
        }
    }

    public void LoadChunkFromBinary(ChunkEntity chunk, string worldName = "main_world" )
    {
        string chunksPath = PathLoader.GetChunksPath(saveName, worldName);
        string chunkFileNmae = $"{chunk.chunkCoord.x},{chunk.chunkCoord.x}.dat";
        string chunkFilePath = Path.Combine(chunksPath, chunkFileNmae);
        if (File.Exists(chunkFilePath))
        {
            using (var reader = new BinaryReader(File.Open(chunkFilePath, FileMode.Open)))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    int x = reader.ReadInt32(); // 假设文件存储的是整数
                    int y = reader.ReadInt32();
                    int z = reader.ReadInt32();
                    string value = reader.ReadString();
                    chunk.blocks[x, y, z] = new BlockEntity(chunk, new(x, y, z), value);
                }
            }
        }else{

        }
    }
}
