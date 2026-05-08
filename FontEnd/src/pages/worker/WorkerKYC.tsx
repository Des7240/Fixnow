import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { Upload, message } from 'antd';
import { Upload as UploadIcon, FileBadge } from 'lucide-react';
import axiosInstance from '../../utils/axiosInstance';
import type { UploadFile, UploadProps } from 'antd/es/upload/interface';

export default function WorkerKYC() {
  const [loading, setLoading] = useState(false);
  const [frontImage, setFrontImage] = useState<UploadFile[]>([]);
  const [backImage, setBackImage] = useState<UploadFile[]>([]);
  const [selfieImage, setSelfieImage] = useState<UploadFile[]>([]);

  const { register, handleSubmit, formState: { errors } } = useForm({
    defaultValues: { citizenIdNumber: '' }
  });

  const onSubmit = async (data: any) => {
    if (frontImage.length === 0 || backImage.length === 0 || selfieImage.length === 0) {
      message.error('Vui lòng upload đủ 3 ảnh bắt buộc');
      return;
    }

    setLoading(true);
    const formData = new FormData();
    formData.append('CitizenIdNumber', data.citizenIdNumber);
    formData.append('FrontImage', frontImage[0].originFileObj as Blob);
    formData.append('BackImage', backImage[0].originFileObj as Blob);
    formData.append('SelfieImage', selfieImage[0].originFileObj as Blob);

    try {
      await axiosInstance.post('/workers/kyc', formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      });
      message.success('Đã gửi hồ sơ KYC thành công. Đang chờ duyệt!');
    } catch (err: any) {
      message.error(err?.response?.data?.message || 'Có lỗi xảy ra');
    } finally {
      setLoading(false);
    }
  };

  const uploadProps = (fileList: UploadFile[], setFileList: any): UploadProps => ({
    onRemove: (file) => {
      const index = fileList.indexOf(file);
      const newFileList = fileList.slice();
      newFileList.splice(index, 1);
      setFileList(newFileList);
    },
    beforeUpload: (file) => {
      setFileList([file]);
      return false; // Prevent auto upload
    },
    fileList,
    maxCount: 1,
    listType: "picture-card",
  });

  return (
    <div className="min-h-full bg-gray-50 flex flex-col">
      <div className="bg-white px-6 pt-10 pb-4 shadow-sm z-10 sticky top-0 flex items-center gap-3">
        <FileBadge className="w-6 h-6 text-orange-500" />
        <h1 className="text-2xl font-bold text-gray-900">Hồ sơ Xác thực (KYC)</h1>
      </div>

      <div className="flex-1 p-6 overflow-y-auto pb-24">
        <div className="bg-orange-50 text-orange-800 p-4 rounded-2xl text-sm mb-6">
          Bạn cần hoàn thành KYC để được xét duyệt tham gia hệ thống FixNow.
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
          <div className="bg-white p-5 rounded-3xl shadow-sm border border-gray-100">
            <label className="block text-sm font-bold text-gray-900 mb-2">Số CCCD / CMND</label>
            <input
              {...register('citizenIdNumber', { required: 'Không được để trống', minLength: { value: 9, message: 'Ít nhất 9 số' } })}
              className="w-full bg-gray-50 rounded-xl px-4 py-3 text-gray-900 focus:outline-none focus:ring-2 focus:ring-orange-500/50"
              placeholder="Nhập 12 số CCCD"
            />
            {errors.citizenIdNumber && <p className="text-red-500 text-xs mt-1">{errors.citizenIdNumber.message as string}</p>}
          </div>

          <div className="bg-white p-5 rounded-3xl shadow-sm border border-gray-100">
            <h3 className="block text-sm font-bold text-gray-900 mb-4">Upload Hình Ảnh</h3>
            
            <div className="space-y-4">
              <div>
                <p className="text-xs text-gray-500 mb-2">Mặt trước CCCD</p>
                <Upload {...uploadProps(frontImage, setFrontImage)}>
                  {frontImage.length === 0 && (
                    <div className="flex flex-col items-center justify-center text-gray-400 gap-1">
                      <UploadIcon className="text-xl w-5 h-5" />
                      <span className="text-xs">Tải ảnh lên</span>
                    </div>
                  )}
                </Upload>
              </div>

              <div>
                <p className="text-xs text-gray-500 mb-2">Mặt sau CCCD</p>
                <Upload {...uploadProps(backImage, setBackImage)}>
                  {backImage.length === 0 && (
                    <div className="flex flex-col items-center justify-center text-gray-400 gap-1">
                      <UploadIcon className="text-xl w-5 h-5" />
                      <span className="text-xs">Tải ảnh lên</span>
                    </div>
                  )}
                </Upload>
              </div>

              <div>
                <p className="text-xs text-gray-500 mb-2">Ảnh Selfie chân dung</p>
                <Upload {...uploadProps(selfieImage, setSelfieImage)}>
                  {selfieImage.length === 0 && (
                    <div className="flex flex-col items-center justify-center text-gray-400 gap-1">
                      <UploadIcon className="text-xl w-5 h-5" />
                      <span className="text-xs">Tải ảnh lên</span>
                    </div>
                  )}
                </Upload>
              </div>
            </div>
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full py-4 bg-orange-500 hover:bg-orange-600 text-white font-bold rounded-2xl shadow-lg shadow-orange-500/30 flex items-center justify-center gap-2"
          >
            {loading ? 'Đang tải lên...' : 'Gửi yêu cầu xét duyệt'}
          </button>
        </form>
      </div>
    </div>
  );
}
